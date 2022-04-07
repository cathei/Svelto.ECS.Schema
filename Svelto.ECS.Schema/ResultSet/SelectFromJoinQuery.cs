using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;
using Svelto.ObjectPool;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct SelectFromJoinQuery<TRow, TResult, TJoined>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TJoined : struct, IResultSet
    {
        internal readonly ResultSetQueryConfig config;

        internal SelectFromJoinQuery(ResultSetQueryConfig config)
        {
            this.config = config;
        }

        public SelectFromJoinOnQuery<TRow, TResult, TJoined, TComponent> On<TComponent>(
                IJoinProvider<TComponent, TRow, IQueryableRow<TJoined>> joiner)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            return new(config, joiner);
        }
    }

    public readonly ref struct SelectFromJoinOnQuery<TRow, TResult, TJoined, TJoinComponent>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TJoined : struct, IResultSet
        where TJoinComponent : unmanaged, IForeignKeyComponent
    {
        internal readonly ResultSetQueryConfig config;
        internal readonly IJoinProvider joiner;

        internal SelectFromJoinOnQuery(ResultSetQueryConfig config, IJoinProvider joiner)
        {
            this.config = config;
            this.joiner = joiner;
        }

        public JoinQueryEnumerator<TResult, TJoined, TJoinComponent> GetEnumerator()
        {
            return new(config, joiner);
        }

        public SelectFromJoinQuery<TRow, TResult, TJoined, TJoined2, TJoinComponent> Join<TJoined2>()
            where TJoined2 : struct, IResultSet
        {
            return new(config, joiner);
        }
    }

    public class SelectFromJoinQuery<TRow, TResult, TJoined1, TJoined2, TJoinComponent1>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TJoined1 : struct, IResultSet
        where TJoined2 : struct, IResultSet
        where TJoinComponent1 : unmanaged, IForeignKeyComponent
    {
        internal readonly ResultSetQueryConfig config;
        internal readonly IJoinProvider joiner1;

        internal SelectFromJoinQuery(ResultSetQueryConfig config, IJoinProvider joiner1)
        {
            this.config = config;
            this.joiner1 = joiner1;
        }

        public SelectFromJoinOnQuery<TRow, TResult, TJoined1, TJoined2, TJoinComponent1, TJoinComponent2> On<TJoinComponent2>(
                IJoinProvider<TJoinComponent2, TRow, IQueryableRow<TJoined2>> joiner2)
            where TJoinComponent2 : unmanaged, IForeignKeyComponent
        {
            return new(config, joiner1, joiner2);
        }
    }

    public readonly ref struct SelectFromJoinOnQuery<TRow, TResult, TJoined1, TJoined2, TJoinComponent1, TJoinComponent2>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TJoined1 : struct, IResultSet
        where TJoined2 : struct, IResultSet
        where TJoinComponent1 : unmanaged, IForeignKeyComponent
        where TJoinComponent2 : unmanaged, IForeignKeyComponent
    {
        internal readonly ResultSetQueryConfig config;
        internal readonly IJoinProvider joiner1;
        internal readonly IJoinProvider joiner2;

        internal SelectFromJoinOnQuery(ResultSetQueryConfig config, IJoinProvider joiner1, IJoinProvider joiner2)
        {
            this.config = config;
            this.joiner1 = joiner1;
            this.joiner2 = joiner2;
        }

        public JoinQueryEnumerator<TResult, TJoined1, TJoined2, TJoinComponent1, TJoinComponent2> GetEnumerator()
        {
            return new(config, joiner1, joiner2);
        }
    }
}
