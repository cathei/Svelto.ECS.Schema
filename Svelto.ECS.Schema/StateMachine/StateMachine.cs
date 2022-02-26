using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public abstract partial class StateMachine<TState> : IEntityStateMachine
        where TState : unmanaged, IKeyEquatable<TState>
    {
        internal FasterDictionary<IKeyEquatable<TState>.Wrapper, StateConfig> _states;

        protected StateMachine()
        {
            _states = new FasterDictionary<IKeyEquatable<TState>.Wrapper, StateConfig>();
        }

        protected internal delegate bool Predicate<TComponent>(ref TComponent component)
            where TComponent : unmanaged, IEntityComponent;

        internal abstract class ConditionConfig
        {
            internal ConditionConfig() { }

            internal abstract void Evaluate(EntitiesDB entitiesDB, NB<Component> state, in IndexedIndices indexedIndices, in ExclusiveGroupStruct groupID);
        }

        internal sealed class ConditionConfig<TComponent> : ConditionConfig
            where TComponent : unmanaged, IEntityComponent
        {
            internal readonly Predicate<TComponent> _predicate;

            public ConditionConfig(Predicate<TComponent> predicate)
            {
                _predicate = predicate;
            }

            internal override void Evaluate(EntitiesDB entitiesDB, NB<Component> state, in IndexedIndices indices, in ExclusiveGroupStruct groupID)
            {
                // this is calling per group here, for this condition
                var (target, _) = entitiesDB.QueryEntities<TComponent>(groupID);

                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    // just skip any component that is not available
                    if (state[i].nextTransition != TransitionAvailable)
                        continue;

                    if (!_predicate(ref target[i]))
                        state[i].nextTransition = TransitionAborted;
                }
            }
        }

        internal sealed class TransitionConfig
        {
            internal readonly StateConfig _current;

            // ID in this state
            internal readonly int _transitionID;

            internal readonly TState _next;
            internal readonly FasterList<ConditionConfig> _conditions;

            internal TransitionConfig(StateConfig current, int transitionID, in TState next)
            {
                _current = current;
                _transitionID = transitionID;
                _next = next;
                _conditions = new FasterList<ConditionConfig>();
            }

            internal void Evaluate(EntitiesDB entitiesDB, NB<Component> state, in IndexedIndices indices, in ExclusiveGroupStruct groupID)
            {
                // rather loop through indexes multiple times.
                // should be better than fetching buffers per entity.
                foreach (int i in indices)
                {
                    if (state[i].nextTransition == TransitionAborted)
                        state[i].nextTransition = TransitionAvailable;
                }

                // evaluate each condition, they will check with components
                for (int i = 0; i < _conditions.count; ++i)
                {
                    _conditions[i].Evaluate(entitiesDB, state, indices, groupID);
                }

                // now if nextTransition is still available, this transition is selected
                foreach (int i in indices)
                {
                    if (state[i].nextTransition == TransitionAvailable)
                        state[i].nextTransition = TransitionConfimed + _transitionID;
                }
            }
        }

        internal sealed class StateConfig
        {
            internal readonly TState _state;
            internal readonly FasterList<TransitionConfig> _transitions;

            internal StateConfig(in TState state)
            {
                _state = state;
                _transitions = new FasterList<TransitionConfig>();
            }

            internal void Evaluate(IndexesDB indexesDB, NB<Component> state, in ExclusiveGroupStruct groupID)
            {
                var indices = Index.Query(_state).From(groupID).Indices(indexesDB);

                for (int i = 0; i < _transitions.count; ++i)
                {
                    _transitions[i].Evaluate(indexesDB.entitiesDB, state, indices, groupID);
                }
            }
        }
    }
}