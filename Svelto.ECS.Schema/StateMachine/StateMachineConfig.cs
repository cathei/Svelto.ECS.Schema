using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    internal abstract class StateMachineConfigBase<TComponent>
        where TComponent : unmanaged, IStateMachineComponent
    {
        internal abstract IStepEngine AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        internal abstract void Process(IndexedDB indexedDB);

        // this will manage filters for state machine
        internal readonly Index<TComponent> _index;

        protected StateMachineConfigBase()
        {
            _index = new();
        }
    }

    internal sealed partial class StateMachineConfig<TRow, TComponent, TState> : StateMachineConfigBase<TComponent>
        where TRow : class, StateMachine<TComponent>.IIndexableRow
        where TComponent : unmanaged, IStateMachineComponent<TState>
        where TState : unmanaged, IEquatable<TState>
    {
        public static StateMachineConfig<TRow, TComponent, TState> Get(StateMachine<TComponent> fsm)
        {
            if (fsm.config is StateMachineConfig<TRow, TComponent, TState> result)
                return result;

            result = new StateMachineConfig<TRow, TComponent, TState>();
            fsm.config = result;
            return result;
        }

        internal readonly FasterDictionary<TState, State> _states;
        internal readonly AnyState _anyState;

        internal StateMachineConfig() : base()
        {
            _states = new();
            _anyState = new(this);
        }

        internal override IStepEngine AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            // this is required to handle added or removed entities
            var indexingEngine = new TableIndexingEngine<
                StateMachine<TComponent>.IIndexableRow, TComponent>(indexedDB);

            // this is required to validate and change state
            var stepEngine = new TransitionEngine(indexedDB, this);

            enginesRoot.AddEngine(indexingEngine);

            enginesRoot.AddEngine(stepEngine);

            return stepEngine;
        }

        internal override void Process(IndexedDB indexedDB)
        {
            var states = _states.GetValues(out var stateCount);

            // clear all filters before proceed
            // we'll have to make sure because engine can called multiple times
            for (int i = 0; i < stateCount; ++i)
            {
                indexedDB.Memo(states[i]._exitCandidates).Clear();
                indexedDB.Memo(states[i]._enterCandidates).Clear();
            }

            foreach (var result in indexedDB.Select<ResultSet<TComponent>>().From<TRow>())
            {
                for (int i = 0; i < stateCount; ++i)
                {
                    states[i].Evaluate(indexedDB, result);
                }

                // any state transition has lower priority
                _anyState.Evaluate(indexedDB, result);

                // check for exit candidates
                for (int i = 0; i < stateCount; ++i)
                {
                    states[i].ProcessExit(indexedDB, result);
                }

                // check for enter candidates
                for (int i = 0; i < stateCount; ++i)
                {
                    states[i].ProcessEnter(indexedDB, result);
                }
            }
        }
    }
}