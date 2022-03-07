using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public partial class StateMachine<TKey>
    {
        void IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            // this is required to handle added or removed entities
            enginesRoot.AddEngine(new TableIndexingEngine<IIndexedRow, TKey, Component>(indexedDB));

            // this is required to validate and change state
            Engine = new TransitionEngine(indexedDB);

            enginesRoot.AddEngine(Engine);
        }

        internal abstract class StateMachineConfigBase
        {
            internal readonly FasterDictionary<KeyWrapper<TKey>, StateConfig> _states;
            internal readonly AnyStateConfig _anyState;

            // this will manage filters for state machine
            internal readonly Index _index;

            protected StateMachineConfigBase()
            {
                _states = new FasterDictionary<KeyWrapper<TKey>, StateConfig>();
                _anyState = new AnyStateConfig(this);
                _index = new Index();
            }

            public abstract void Process(IndexedDB indexedDB);
        }

        internal sealed class StateMachineConfig<TRow> : StateMachineConfigBase
            where TRow : class, IIndexedRow
        {
            public override void Process(IndexedDB indexedDB)
            {
                var tables = indexedDB.FindTables<TRow>();
                var states = _states.GetValues(out var stateCount);

                // clear all filters before proceed
                // maybe not needed with new filter system
                // but again we have to make sure because engine can called multiple times
                for (int i = 0; i < stateCount; ++i)
                {
                    indexedDB.Memo(states[i]._exitCandidates).Clear();
                    indexedDB.Memo(states[i]._enterCandidates).Clear();
                }

                foreach (var result in indexedDB.Select<IndexableResultSet<Component>>().From(tables).Entities())
                {
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].Evaluate(indexedDB, result.set.component, result.table);
                    }

                    // any state transition has lower priority
                    _anyState.Evaluate(indexedDB, result.set.component, result.set.count, result.table);

                    // check for exit candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessExit(indexedDB, result.table);
                    }

                    // check for enter candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessEnter(indexedDB, result.set.component, result.table);
                    }
                }
            }
        }

        private sealed class TransitionEngine : IStepEngine
        {
            private readonly IndexedDB _indexedDB;

            public string name { get; } = $"{typeof(StateMachine<TKey>).FullName}.TransitionEngine";

            internal TransitionEngine(IndexedDB indexedDB)
            {
                _indexedDB = indexedDB;
            }

            public void Step()
            {
                Config.Process(_indexedDB);
            }
        }
    }
}
