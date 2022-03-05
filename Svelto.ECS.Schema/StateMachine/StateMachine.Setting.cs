using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public partial class StateMachine<TState>
    {
        protected Builder<TRow> Configure<TRow>()
            where TRow : class, IIndexedRow
        {
            if (Config != null)
                throw new ECSException("Configure should only called once!");

            var config = new StateMachineConfig<TRow>();
            Config = config;
            return new Builder<TRow>(config);
        }

        public readonly ref struct Builder<TRow>
            where TRow : class, IEntityRow<Component>
        {
            private readonly StateMachineConfigBase _config;

            internal Builder(StateMachineConfigBase config)
            {
                _config = config;
            }

            public AnyStateBuilder AnyState => new AnyStateBuilder(_config._anyState);

            public StateBuilder AddState(in TState state)
            {
                var wrapper = new KeyWrapper<TState>(state);

                if (_config._states.ContainsKey(wrapper))
                {
                    throw new ECSException($"State {state} already exsists!");
                }

                var stateConfig = new StateConfig(state);
                _config._states[wrapper] = stateConfig;

                return new StateBuilder(stateConfig);
            }

            public readonly ref struct StateBuilder
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
            }

            public readonly ref struct TransitionBuilder
            {
                internal readonly TransitionConfig _transition;

                internal TransitionBuilder(TransitionConfig transitionConfig)
                {
                    _transition = transitionConfig;
                }
            }

            public readonly ref struct AnyStateBuilder
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

    public static class StateMachineConfigExtensions
    {
        public static StateMachine<TS>.Builder<TR>.TransitionBuilder AddCondition<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<TC> preciate)
            where TS : unmanaged, IStateMachineKey<TS>
            where TR : class, IEntityRow<TC>, IEntityRow<StateMachine<TS>.Component>
            where TC : unmanaged, IEntityComponent
        {
            var condition = new StateMachine<TS>.ConditionConfigNative<TC>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.TransitionBuilder AddCondition<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<TC> preciate)
            where TS : unmanaged, IStateMachineKey<TS>
            where TR : class, IEntityRow<TC>, IEntityRow<StateMachine<TS>.Component>
            where TC : struct, IEntityViewComponent
        {
            var condition = new StateMachine<TS>.ConditionConfigManaged<TC>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackNative<TC> callback)
            where TS : unmanaged, IStateMachineKey<TS>
            where TR : class, IEntityRow<TC>, IEntityRow<StateMachine<TS>.Component>
            where TC : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS>.CallbackConfigNative<TC>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackManaged<TC> callback)
            where TS : unmanaged, IStateMachineKey<TS>
            where TR : class, IEntityRow<TC>, IEntityRow<StateMachine<TS>.Component>
            where TC : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS>.CallbackConfigManaged<TC>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackNative<TC> callback)
            where TS : unmanaged, IStateMachineKey<TS>
            where TR : class, IEntityRow<TC>, IEntityRow<StateMachine<TS>.Component>
            where TC : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS>.CallbackConfigNative<TC>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackManaged<TC> callback)
            where TS : unmanaged, IStateMachineKey<TS>
            where TR : class, IEntityRow<TC>, IEntityRow<StateMachine<TS>.Component>
            where TC : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS>.CallbackConfigManaged<TC>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }
    }
}