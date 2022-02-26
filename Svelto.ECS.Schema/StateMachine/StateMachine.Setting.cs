using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState> : IEntityStateMachine
    {
        protected StateBuilder AddState(in TState state)
        {
            var wrapper = new IKeyEquatable<Key>.Wrapper(new Key(state));

            if (_states.ContainsKey(wrapper))
            {
                throw new ECSException($"State {state} already exsists!");
            }

            var stateConfig = new StateConfig(state);
            _states[wrapper] = stateConfig;

            return new StateBuilder(stateConfig);
        }

        protected readonly ref struct StateBuilder
        {
            internal readonly StateConfig _state;

            internal StateBuilder(StateConfig stateConfig)
            {
                _state = stateConfig;
            }

            public StateTransitionBuilder AddTransition(in TState next)
            {
                var transition = new TransitionConfig(_state, _state._transitions.count, next);
                _state._transitions.Add(transition);
                return new StateTransitionBuilder(this, transition);
            }
        }

        protected readonly ref struct StateTransitionBuilder
        {
            internal readonly StateBuilder _state;
            internal readonly TransitionConfig _transition;

            internal StateTransitionBuilder(in StateBuilder stateBuilder, TransitionConfig transitionConfig)
            {
                _state = stateBuilder;
                _transition = transitionConfig;
            }

            public StateTransitionBuilder AddCondition<TComponent>(Predicate<TComponent> preciate)
                where TComponent : unmanaged, IEntityComponent
            {
                var condition = new ConditionConfig<TComponent>(preciate);
                _transition._conditions.Add(condition);
                return this;
            }

            // fluent api
            public StateTransitionBuilder AddTransition(in TState next)
            {
                return _state.AddTransition(next);
            }
        }
    }
}