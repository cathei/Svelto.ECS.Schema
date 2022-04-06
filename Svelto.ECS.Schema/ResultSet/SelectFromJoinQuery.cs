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

        public SelectFromJoinQueryEnumerator<TResult, TJoined, TJoinComponent> GetEnumerator()
        {
            return new(config, joiner);
        }

    }
}
