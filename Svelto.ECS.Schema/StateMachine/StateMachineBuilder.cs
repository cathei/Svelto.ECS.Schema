using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public readonly ref struct StateMachineBuilder<TRow, TComponent>
        where TRow : class, IEntityRow<TComponent>
        where TComponent : unmanaged, IStateMachineComponent
    {
        public AnyStateBuilder AnyState => new AnyStateBuilder();

        public readonly ref struct StateBuilder<TState>
            where TState : unmanaged, IEquatable<TState>
        {
            internal readonly TState _key;

            internal StateBuilder(TState key)
            {
                _key = key;
            }

            public TransitionBuilder AddTransition(in TComponent next)
            {
                var transition = new TransitionConfig(next);
                _state._transitions.Add(transition);
                return new TransitionBuilder(transition);
            }

            public TransitionBuilder AddTransition(in StateBuilder next)
                => AddTransition(next._state._key);
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

            public TransitionBuilder AddTransition(in TComponent next)
            {
                var transition = new TransitionConfig(next);
                _state._transitions.Add(transition);
                return new TransitionBuilder(transition);
            }

            public TransitionBuilder AddTransition(in StateBuilder next)
                => AddTransition(next._state._key);
        }
    }

    public static class StateMachineConfigExtensions
    {
        public static StateMachineBuilder<TRow, TComponent>.StateBuilder<TState> AddState<TRow, TComponent, TState>(
                this StateMachineBuilder<TRow, TComponent> builder, in TState key)
            where TRow : class, StateMachine<TComponent>.IIndexableRow
            where TComponent : unmanaged, IStateMachineComponent<TState>
            where TState : unmanaged, IEquatable<TState>
        {
            var config = StateMachineConfig<TRow, TComponent, TState>.Default;

            if (config._states.ContainsKey(key))
            {
                throw new ECSException($"State {key} already exsists!");
            }

            var stateConfig = new StateConfig(key);
            config._states[key] = stateConfig;

            return new StateBuilder(stateConfig);
        }





        public static StateMachine<TS>.Builder<TR>.TransitionBuilder AddCondition<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<TC> preciate)
            where TS : unmanaged, IStateMachineComponent
            where TR : class, IEntityRow<TC>, IEntityRow<TS>
            where TC : unmanaged, IEntityComponent
        {
            var condition = new StateMachine<TS>.ConditionConfigNative<TC>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.TransitionBuilder AddCondition<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<TC> preciate)
            where TS : unmanaged, IStateMachineComponent
            where TR : class, IEntityRow<TC>, IEntityRow<TS>
            where TC : struct, IEntityViewComponent
        {
            var condition = new StateMachine<TS>.ConditionConfigManaged<TC>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackNative<TC> callback)
            where TS : unmanaged, IStateMachineComponent
            where TR : class, IEntityRow<TC>, IEntityRow<TS>
            where TC : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS>.CallbackConfigNative<TC>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackManaged<TC> callback)
            where TS : unmanaged, IStateMachineComponent
            where TR : class, IEntityRow<TC>, IEntityRow<TS>
            where TC : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS>.CallbackConfigManaged<TC>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackNative<TC> callback)
            where TS : unmanaged, IStateMachineComponent
            where TR : class, IEntityRow<TC>, IEntityRow<TS>
            where TC : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS>.CallbackConfigNative<TC>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TR, TC>(
                this StateMachine<TS>.Builder<TR>.StateBuilder builder,
                CallbackManaged<TC> callback)
            where TS : unmanaged, IStateMachineComponent
            where TR : class, IEntityRow<TC>, IEntityRow<TS>
            where TC : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS>.CallbackConfigManaged<TC>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }
    }
}