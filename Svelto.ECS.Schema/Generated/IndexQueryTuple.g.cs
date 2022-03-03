﻿ // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    public static class StateMachineConfigNativeExtensions
    {

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : unmanaged, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : unmanaged, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : unmanaged, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : unmanaged, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : unmanaged, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : unmanaged, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<T1> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : unmanaged, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : unmanaged, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigNative<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

    }

    public static class StateMachineConfigManagedExtensions
    {

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent                where T3 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent                where T3 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityViewComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityViewComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityViewComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityViewComponent                where T4 : struct, IEntityComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityViewComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityViewComponent                where T4 : struct, IEntityComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<T1> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityViewComponent
        {
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<T1>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, T1, T2, T3, T4>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<T1> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<T1>
                where T1 : struct, IEntityComponent                where T2 : struct, IEntityComponent                where T3 : struct, IEntityComponent                where T4 : struct, IEntityViewComponent
        {
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<T1>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }

    }
}