using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState, TUnique>
    {
        protected AnyStateBuilder AnyState => new AnyStateBuilder(Config.AnyState);

        protected StateBuilder AddState(in TState state)
        {
            var wrapper = new KeyWrapper<TState>(state);

            if (Config.States.ContainsKey(wrapper))
            {
                throw new ECSException($"State {state} already exsists!");
            }

            var stateConfig = new StateConfig(state);
            Config.States[wrapper] = stateConfig;

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
                var transition = new TransitionConfig(next);
                _state._transitions.Add(transition);
                return new TransitionBuilder(transition);
            }

            public TransitionBuilder AddTransition(in StateBuilder next)
                => AddTransition(next._state._state);

            public StateBuilder ExecuteOnExit<TComponent>(CallbackNative<TComponent> callback)
                where TComponent : unmanaged, IEntityComponent
            {
                var config = new CallbackConfigNative<TComponent>(callback);
                _state._onExit.Add(config);
                return this;
            }

            public StateBuilder ExecuteOnExit<TComponent>(CallbackManaged<TComponent> callback)
                where TComponent : struct, IEntityViewComponent
            {
                var config = new CallbackConfigManaged<TComponent>(callback);
                _state._onExit.Add(config);
                return this;
            }

            public StateBuilder ExecuteOnEnter<TComponent>(CallbackNative<TComponent> callback)
                where TComponent : unmanaged, IEntityComponent
            {
                var config = new CallbackConfigNative<TComponent>(callback);
                _state._onEnter.Add(config);
                return this;
            }

            public StateBuilder ExecuteOnEnter<TComponent>(CallbackManaged<TComponent> callback)
                where TComponent : struct, IEntityViewComponent
            {
                var config = new CallbackConfigManaged<TComponent>(callback);
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

            public TransitionBuilder AddCondition<TComponent>(PredicateNative<TComponent> preciate)
                where TComponent : unmanaged, IEntityComponent
            {
                var condition = new ConditionConfigNative<TComponent>(preciate);
                _transition._conditions.Add(condition);
                return this;
            }

            public TransitionBuilder AddCondition<TComponent>(PredicateManaged<TComponent> preciate)
                where TComponent : struct, IEntityViewComponent
            {
                var condition = new ConditionConfigManaged<TComponent>(preciate);
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
                var transition = new TransitionConfig(next);
                _state._transitions.Add(transition);
                return new TransitionBuilder(transition);
            }

            public TransitionBuilder AddTransition(in StateBuilder next)
                => AddTransition(next._state._state);
        }
    }
}