using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct StateMachineBuilder<TRow, TComponent>
        where TRow : class, IEntityRow<TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        internal readonly StateMachine<TComponent> _fsm;

        public StateMachineBuilder(StateMachine<TComponent> fsm)
        {
            _fsm = fsm;
        }

        public AnyStateBuilder AnyState => new AnyStateBuilder(_fsm);

        public readonly ref struct AnyStateBuilder
        {
            internal readonly StateMachine<TComponent> _fsm;

            public AnyStateBuilder(StateMachine<TComponent> fsm)
            {
                _fsm = fsm;
            }
        }
    }

    public readonly ref struct StateBuilder<TRow, TComponent, TState>
        where TRow : class, StateMachine<TComponent>.IStateMachineRow
        where TComponent : unmanaged, IKeyComponent<TState>
        where TState : unmanaged, IEquatable<TState>
    {
        internal readonly StateMachineConfig<TRow, TComponent, TState>.State _state;

        internal StateBuilder(StateMachineConfig<TRow, TComponent, TState>.State state)
        {
            _state = state;
        }

        public TransitionBuilder<TRow, TState> AddTransition(in TState next)
        {
            var transition = new TransitionConfig<TState>(next);
            _state._transitions.Add(transition);
            return new TransitionBuilder<TRow, TState>(transition);
        }

        public static implicit operator TState(in StateBuilder<TRow, TComponent, TState> builder)
            => builder._state._key;
    }

    public readonly ref struct TransitionBuilder<TRow, TState>
    {
        internal readonly TransitionConfig<TState> _transition;

        internal TransitionBuilder(TransitionConfig<TState> transitionConfig)
        {
            _transition = transitionConfig;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public static class StateMachineConfigExtensions
    {
        public static StateBuilder<TRow, TComponent, TState> AddState<TRow, TComponent, TState>(
                this StateMachineBuilder<TRow, TComponent> builder, in TState key)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
        {
            var config = StateMachineConfig<TRow, TComponent, TState>.Get(builder._fsm);

            if (config._states.ContainsKey(key))
            {
                throw new ECSException($"State {key} already exsists!");
            }

            var stateConfig = new StateMachineConfig<TRow, TComponent, TState>.State(config, key);
            config._states[key] = stateConfig;

            return new StateBuilder<TRow, TComponent, TState>(stateConfig);
        }

        public static TransitionBuilder<TRow, TState> AddTransition<TRow, TComponent, TState>(
                this StateMachineBuilder<TRow, TComponent>.AnyStateBuilder builder, in TState next)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
        {
            var config = StateMachineConfig<TRow, TComponent, TState>.Get(builder._fsm);

            var transition = new TransitionConfig<TState>(next);
            config._anyState._transitions.Add(transition);
            return new TransitionBuilder<TRow, TState>(transition);
        }

        public static TransitionBuilder<TRow, TState> AddCondition<TRow, TState, TCondition>(
                this TransitionBuilder<TRow, TState> builder, PredicateNative<TCondition> preciate)
            where TRow : class, IEntityRow<TCondition>
            where TCondition : unmanaged, IEntityComponent
        {
            var condition = new ConditionConfigNative<TCondition>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static TransitionBuilder<TRow, TState> AddCondition<TRow, TState, TCondition>(
                this TransitionBuilder<TRow, TState> builder, PredicateManaged<TCondition> preciate)
            where TRow : class, IEntityRow<TCondition>
            where TCondition : struct, IEntityViewComponent
        {
            var condition = new ConditionConfigManaged<TCondition>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateBuilder<TRow, TComponent, TState> ExecuteOnExit<TRow, TComponent, TState, TCallback>(
                this StateBuilder<TRow, TComponent, TState> builder, CallbackNative<TCallback> callback)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow, IEntityRow<TCallback>
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
            where TCallback : unmanaged, IEntityComponent
        {
            var config = new CallbackConfigNative<TCallback>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateBuilder<TRow, TComponent, TState> ExecuteOnExit<TRow, TComponent, TState, TCallback>(
                this StateBuilder<TRow, TComponent, TState> builder, CallbackManaged<TCallback> callback)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow, IEntityRow<TCallback>
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
            where TCallback : struct, IEntityViewComponent
        {
            var config = new CallbackConfigManaged<TCallback>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateBuilder<TRow, TComponent, TState> ExecuteOnEnter<TRow, TComponent, TState, TCallback>(
                this StateBuilder<TRow, TComponent, TState> builder, CallbackNative<TCallback> callback)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow, IEntityRow<TCallback>
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
            where TCallback : unmanaged, IEntityComponent
        {
            var config = new CallbackConfigNative<TCallback>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateBuilder<TRow, TComponent, TState> ExecuteOnEnter<TRow, TComponent, TState, TCallback>(
                this StateBuilder<TRow, TComponent, TState> builder, CallbackManaged<TCallback> callback)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow, IEntityRow<TCallback>
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
            where TCallback : struct, IEntityViewComponent
        {
            var config = new CallbackConfigManaged<TCallback>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }
    }

    public static class StateMachineConfigEnumExtensions
    {
        // use enum as key
        public static StateBuilder<TRow, TComponent, EnumKey<TState>> AddState<TRow, TComponent, TState>(
                this StateMachineBuilder<TRow, TComponent> builder, in TState key)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow
            where TComponent : unmanaged, IKeyComponent<EnumKey<TState>>
            where TState : unmanaged, Enum
        {
            return builder.AddState((EnumKey<TState>)key);
        }

        // use enum as key
        public static TransitionBuilder<TRow, EnumKey<TState>> AddTransition<TRow, TComponent, TState>(
                this StateMachineBuilder<TRow, TComponent>.AnyStateBuilder builder, in TState next)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow
            where TComponent : unmanaged, IKeyComponent<EnumKey<TState>>
            where TState : unmanaged, Enum
        {
            return builder.AddTransition((EnumKey<TState>)next);
        }
    }

    public static class StateMachineConfigBuilderExtensions
    {
        // use state builder as key
        public static TransitionBuilder<TRow, TState> AddTransition<TRow, TComponent, TState>(
                this StateMachineBuilder<TRow, TComponent>.AnyStateBuilder builder, StateBuilder<TRow, TComponent, TState> next)
            where TRow : class, StateMachine<TComponent>.IStateMachineRow
            where TComponent : unmanaged, IKeyComponent<TState>
            where TState : unmanaged, IEquatable<TState>
        {
            return builder.AddTransition((TState)next);
        }
    }
}