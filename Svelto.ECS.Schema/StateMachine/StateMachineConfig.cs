using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public struct StateMachineResultSet<TComponent> : IResultSet<TComponent>
        where TComponent : unmanaged, IStateMachineComponent
    {
        public NB<TComponent> component;

        public int count { get; set; }

        public void Init(in EntityCollection<TComponent> buffers)
        {
            (component, count) = buffers;
        }
    }

    internal abstract class StateMachineConfigBase<TComponent>
        where TComponent : unmanaged, IStateMachineComponent
    {
        internal abstract IStepEngine AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        internal abstract void Process(IndexedDB indexedDB);

        // this will manage filters for state machine
        internal readonly Index<TComponent> _index;

        protected StateMachineConfigBase()
        {
            _index = new Index<TComponent>();
        }
    }

    internal sealed partial class StateMachineConfig<TRow, TComponent, TState> : StateMachineConfigBase<TComponent>
        where TRow : class, StateMachine<TComponent>.IIndexableRow
        where TComponent : unmanaged, IStateMachineComponent<TState>
        where TState : unmanaged, IEquatable<TState>
    {
        // this is real configuration
        internal static StateMachineConfig<TRow, TComponent, TState> Default =
            new StateMachineConfig<TRow, TComponent, TState>();

        static StateMachineConfig()
        {
            // propagate to state machine
            StateMachine<TComponent>.Config = Default;
        }

        internal readonly FasterDictionary<TState, State> _states;
        internal readonly AnyState _anyState;

        internal StateMachineConfig() : base()
        {
            _states = new FasterDictionary<TState, State>();
            _anyState = new AnyState(this);
        }

        internal override IStepEngine AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            // this is required to handle added or removed entities
            var indexingEngine = new TableIndexingEngine<
                StateMachine<TComponent>.IIndexableRow, TComponent, TState>(indexedDB);

            // this is required to validate and change state
            var stepEngine = new TransitionEngine(indexedDB);

            enginesRoot.AddEngine(indexingEngine);

            enginesRoot.AddEngine(stepEngine);

            return stepEngine;
        }

        internal override void Process(IndexedDB indexedDB)
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

            foreach (var result in indexedDB.Select<StateMachineResultSet<TComponent>>().From(tables).Entities())
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
}