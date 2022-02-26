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

            var stateConfig = new StateConfig(this, state);
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

            public TransitionBuilder AddTransition(in TState next)
            {
                var transition = new TransitionConfig(_state._fsm, _state._transitions.count, next);
                _state._transitions.Add(transition);
                return new TransitionBuilder(transition);
            }

            public TransitionBuilder AddTransition(in StateBuilder next)
                => AddTransition(next._state._state);

            public StateBuilder ExecuteOnExit<TComponent>(Callback<TComponent> callback)
                where TComponent : unmanaged, IEntityComponent
            {
                var config = new CallbackConfig<TComponent>(callback);
                _state._onExit.Add(config);
                return this;
            }

            public StateBuilder ExecuteOnEnter<TComponent>(Callback<TComponent> callback)
                where TComponent : unmanaged, IEntityComponent
            {
                var config = new CallbackConfig<TComponent>(callback);
                _state._onEnter.Add(config);
                return this;
            }
        }

        protected readonly ref struct TransitionBuilder
        {
            internal readonly TransitionConfig _transition;

            internal TransitionBuilder(TransitionConfig transitionConfig)
            {
                _transition = transitionConfig;
            }

            public TransitionBuilder AddCondition<TComponent>(Predicate<TComponent> preciate)
                where TComponent : unmanaged, IEntityComponent
            {
                var condition = new ConditionConfig<TComponent>(preciate);
                _transition._conditions.Add(condition);
                return this;
            }
        }

        protected readonly ref struct AnyStateBuilder
        {
            internal readonly AnyStateConfig _state;

            internal AnyStateBuilder(AnyStateConfig stateConfig)
            {
                _state = stateConfig;
            }

            public TransitionBuilder AddTransition(in TState next)
            {
                var transition = new TransitionConfig(_state._fsm, _state._transitions.count, next);
                _state._transitions.Add(transition);
                return new TransitionBuilder(transition);
            }

            public TransitionBuilder AddTransition(in StateBuilder next)
                => AddTransition(next._state._state);
        }
    }
}