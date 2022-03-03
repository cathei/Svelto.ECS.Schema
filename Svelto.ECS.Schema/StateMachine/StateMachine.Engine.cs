using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState, TTag>
    {
        void IEntityStateMachine.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            // this is required to handle added or removed entities
            enginesRoot.AddEngine(new TableIndexingEngine<IIndexedRow, TState, Component>(indexedDB));

            // this is required to validate and change state
            Engine = new TransitionEngine(indexedDB);

            enginesRoot.AddEngine(Engine);
        }


        internal interface IStateMachineConfig
        {
            Index Index { get; }
            void Process(IndexedDB indexedDB);
        }

        internal sealed class StateMachineConfig<TRow> : IStateMachineConfig
            where TRow : class, IIndexedRow
        {
            internal readonly FasterDictionary<KeyWrapper<TState>, StateConfig> _states;
            internal readonly AnyStateConfig _anyState;

            // this will manage filters for state machine
            internal readonly Index _index;

            public Index Index => _index;

            public StateMachineConfig()
            {
                _states = new FasterDictionary<KeyWrapper<TState>, StateConfig>();
                _anyState = new AnyStateConfig();

                _index = new Index();
            }

            public void Process(IndexedDB indexedDB)
            {
                var tables = indexedDB.Select<TRow>().Tables();

                //entitiesDB.FindGroups<Component>();
                var states = _states.GetValues(out var stateCount);

                // clear all filters before proceed
                // maybe not needed with new filter system
                for (int i = 0; i < stateCount; ++i)
                {
                    indexedDB.Memo(states[i]._exitCandidates).Clear();
                    indexedDB.Memo(states[i]._enterCandidates).Clear();
                }

                foreach (var ((component, count), table) in
                    indexedDB.Select<TRow>().From(tables).Entities())
                {
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].Evaluate(this, indexedDB, component, table);
                    }

                    // any state transition has lower priority
                    _anyState.Evaluate(this, indexedDB, component, count, table);

                    // check for exit candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessExit(indexedDB, table);
                    }

                    // check for enter candidates
                    for (int i = 0; i < stateCount; ++i)
                    {
                        states[i].ProcessEnter(indexedDB, component, table);
                    }
                }
            }
        }

        public sealed class TransitionEngine : IStepEngine
        {
            private readonly IndexedDB _indexedDB;

            public string name { get; } = $"{typeof(TTag).FullName}.TransitionEngine";

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
