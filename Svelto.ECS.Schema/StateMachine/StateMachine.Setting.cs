using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState, TTag>
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
            where TRow : class, IIndexedRow
        {
            private readonly StateMachineConfig<TRow> _config;

            internal Builder(StateMachineConfig<TRow> config)
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
        // public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1>(
        //         this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
        //         PredicateNative<T1> preciate)
        //     where TT : unmanaged, StateMachine<TS, TT>.ITag
        //     where TS : unmanaged
        //     where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
        //     where T1 : unmanaged, IEntityComponent
        // {
        //     var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
        //     builder._transition._conditions.Add(condition);
        //     return builder;
        // }

        // public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1>(
        //         this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
        //         PredicateManaged<T1> preciate)
        //     where TS : unmanaged
        //     where TT : unmanaged, StateMachine<TS, TT>.ITag
        //     where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
        //     where T1 : struct, IEntityViewComponent
        // {
        //     var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
        //     builder._transition._conditions.Add(condition);
        //     return builder;
        // }

        // public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1>(
        //         this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
        //         CallbackNative<T1> callback)
        //     where TS : unmanaged
        //     where TT : unmanaged, StateMachine<TS, TT>.ITag
        //     where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
        //     where T1 : unmanaged, IEntityComponent
        // {
        //     var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
        //     builder._state._onExit.Add(config);
        //     return builder;
        // }

        // public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1>(
        //         this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
        //         CallbackManaged<T1> callback)
        //     where TS : unmanaged
        //     where TT : unmanaged, StateMachine<TS, TT>.ITag
        //     where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
        //     where T1 : struct, IEntityViewComponent
        // {
        //     var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
        //     builder._state._onExit.Add(config);
        //     return builder;
        // }

        // public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1>(
        //         this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
        //         CallbackNative<T1> callback)
        //     where TS : unmanaged
        //     where TT : unmanaged, StateMachine<TS, TT>.ITag
        //     where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
        //     where T1 : unmanaged, IEntityComponent
        // {
        //     var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
        //     builder._state._onEnter.Add(config);
        //     return builder;
        // }

        // public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1>(
        //         this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
        //         CallbackManaged<T1> callback)
        //     where TS : unmanaged
        //     where TT : unmanaged, StateMachine<TS, TT>.ITag
        //     where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
        //     where T1 : struct, IEntityViewComponent
        // {
        //     var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
        //     builder._state._onEnter.Add(config);
        //     return builder;
        // }
    }
}