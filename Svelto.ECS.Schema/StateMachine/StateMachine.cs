using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// State machine on Svelto ECS
    /// </summary>
    /// <typeparam name="TState">Value type representing a state. Assumed as enum. Must be unique per state machine.</typeparam>
    public abstract partial class StateMachine<TState> : IEntityStateMachine
        where TState : unmanaged
    {
        // lock is not needed unlike EntitySchemaHolder
        // because no shared memory or reflection etc involved
        internal static readonly StateMachineConfig Config = new StateMachineConfig();
        internal static readonly EqualityComparer<TState> Comparer = EqualityComparer<TState>.Default;

        public TransitionEngine Engine { get; internal set; }

        protected StateMachine()
        {
        }

        protected internal delegate bool Predicate<TComponent>(ref TComponent component)
            where TComponent : unmanaged, IEntityComponent;

        protected internal delegate void Callback<TComponent>(ref TComponent component)
            where TComponent : unmanaged, IEntityComponent;

        internal abstract class ConditionConfig
        {
            internal ConditionConfig() { }

            internal abstract void Evaluate<TEnum, TIter>(EntitiesDB entitiesDB,
                    NB<Component> component, in TEnum indexedIndices, in ExclusiveGroupStruct groupID)
                where TEnum : struct, IIndicesEnumerable<TIter>
                where TIter : struct, IIndicesEnumerator;
        }

        internal sealed class ConditionConfig<TComponent> : ConditionConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly Predicate<TComponent> _predicate;

            public ConditionConfig(Predicate<TComponent> predicate)
            {
                _predicate = predicate;
            }

            internal override void Evaluate<TEnum, TIter>(EntitiesDB entitiesDB,
                NB<Component> component, in TEnum indices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this condition
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    // just skip any component that is not available
                    if (component[i].transitionState != TransitionState.Available)
                        continue;

                    if (!_predicate(ref target[i]))
                        component[i].transitionState = TransitionState.Aborted;
                }
            }
        }

        internal abstract class CallbackConfig
        {
            internal CallbackConfig() { }

            internal abstract void Invoke(EntitiesDB entitiesDB,
                IndexedIndices indexedIndices, in ExclusiveGroupStruct groupID);
        }

        internal sealed class CallbackConfig<TComponent> : CallbackConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly Callback<TComponent> _callback;

            public CallbackConfig(Callback<TComponent> callback)
            {
                _callback = callback;
            }

            internal override void Invoke(EntitiesDB entitiesDB,
                IndexedIndices indexedIndices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this callback
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                foreach (int i in indexedIndices)
                {
                    _callback(ref target[i]);
                }
            }
        }

        internal sealed class TransitionConfig
        {
            // ID in this state
            internal readonly TState _next;
            internal readonly FasterList<ConditionConfig> _conditions;

            internal TransitionConfig(in TState next)
            {
                _next = next;
                _conditions = new FasterList<ConditionConfig>();
            }

            internal void Evaluate<TEnum, TIter>(IndexesDB indexesDB, NB<Component> component, in TEnum indices, in ExclusiveGroupStruct groupID)
                where TEnum : struct, IIndicesEnumerable<TIter>
                where TIter : struct, IIndicesEnumerator
            {
                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    if (component[i].transitionState == TransitionState.Aborted)
                        component[i].transitionState = TransitionState.Available;
                }

                // evaluate each condition, they will check with components
                for (int i = 0; i < _conditions.count; ++i)
                {
                    _conditions[i].Evaluate<TEnum, TIter>(indexesDB.entitiesDB, component, indices, groupID);
                }

                // now if transitionState is still available, this transition is selected
                foreach (int i in indices)
                {
                    if (component[i].transitionState == TransitionState.Available)
                    {
                        component[i].transitionState = TransitionState.Confirmed;

                        var currentState = new IKeyEquatable<Key>.Wrapper(component[i]._key);
                        var nextState = new IKeyEquatable<Key>.Wrapper(_next);

                        Config.States[currentState]._exitCandidates.Add(indexesDB, component[i].ID);
                        Config.States[nextState]._enterCandidates.Add(indexesDB, component[i].ID);
                    }
                }
            }
        }

        internal sealed class StateConfig
        {
            internal readonly StateMachine<TState> _fsm;
            internal readonly TState _state;
            internal readonly FasterList<TransitionConfig> _transitions;

            internal readonly Memo<Component> _exitCandidates;
            internal readonly Memo<Component> _enterCandidates;

            internal readonly FasterList<CallbackConfig> _onExit;
            internal readonly FasterList<CallbackConfig> _onEnter;

            internal StateConfig(StateMachine<TState> fsm, in TState state)
            {
                _fsm = fsm;
                _state = state;
                _transitions = new FasterList<TransitionConfig>();

                _exitCandidates = new Memo<Component>();
                _enterCandidates = new Memo<Component>();

                _onExit = new FasterList<CallbackConfig>();
                _onEnter = new FasterList<CallbackConfig>();
            }

            internal void Evaluate(IndexesDB indexesDB, NB<Component> component, in ExclusiveGroupStruct groupID)
            {
                var indices = Config.Index.Query(_state).From(groupID).Indices(indexesDB);

                // higher priority if added first
                for (int i = 0; i < _transitions.count; ++i)
                {
                    _transitions[i].Evaluate<IndexedIndices, FilteredIndicesEnumerator>
                        (indexesDB, component, indices, groupID);
                }
            }

            internal void ProcessExit(IndexesDB indexesDB, in ExclusiveGroupStruct groupID)
            {
                if (_onExit.count == 0)
                    return;

                var indices = _exitCandidates.From(groupID).Indices(indexesDB);

                if (indices.Count() == 0)
                    return;

                for (int i = 0; i < _onExit.count; ++i)
                {
                    _onExit[i].Invoke(indexesDB.entitiesDB, indices, groupID);
                }
            }

            internal void ProcessEnter(IndexesDB indexesDB, NB<Component> component, in ExclusiveGroupStruct groupID)
            {
                var indices = _enterCandidates.From(groupID).Indices(indexesDB);

                if (indices.Count() == 0)
                    return;

                foreach (int i in indices)
                {
                    // this group will not be visited again in this step
                    // see you next step
                    component[i].transitionState = TransitionState.Available;
                    component[i].Update(indexesDB, _state);
                }

                for (int i = 0; i < _onEnter.count; ++i)
                {
                    _onEnter[i].Invoke(indexesDB.entitiesDB, indices, groupID);
                }
            }
        }

        internal sealed class AnyStateConfig
        {
            internal readonly FasterList<TransitionConfig> _transitions;

            internal AnyStateConfig()
            {
                _transitions = new FasterList<TransitionConfig>();
            }

            internal void Evaluate(IndexesDB indexesDB, NB<Component> state, int count, in ExclusiveGroupStruct groupID)
            {
                var indices = new RangeIndiceEnumerable(count);

                for (int i = 0; i < _transitions.count; ++i)
                {
                    _transitions[i].Evaluate<RangeIndiceEnumerable, RangeIndicesEnumerator>
                        (indexesDB, state, indices, groupID);
                }
            }
        }

        internal sealed class StateMachineConfig
        {
            internal readonly FasterDictionary<IKeyEquatable<Key>.Wrapper, StateConfig> States;
            internal readonly AnyStateConfig AnyState;

            // this will manage filters for state machine
            internal readonly Index Index;

            public StateMachineConfig()
            {
                States = new FasterDictionary<IKeyEquatable<Key>.Wrapper, StateConfig>();
                AnyState = new AnyStateConfig();

                Index = new Index();
            }
        }
    }
}