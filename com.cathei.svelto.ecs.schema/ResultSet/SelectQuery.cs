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
    public struct EmptyResultSet : IResultSet
    {
        public void LoadEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID) { }
    }

    public readonly ref struct SelectQuery<TResult>
        where TResult : struct, IResultSet
    {
        internal readonly IndexedDB _indexedDB;

        public SelectQuery(IndexedDB indexedDB)
        {
            _indexedDB = indexedDB;
        }

        public SelectFromQuery<TRow, TResult> FromAll<TRow>()
            where TRow : class, IQueryableRow<TResult>
        {
            return new(_indexedDB);
        }

        public SelectFromQuery<TRow, TResult> From<TRow>(IEntityTables<TRow> tables)
            where TRow : class, IQueryableRow<TResult>
        {
            return new(_indexedDB, tables);
        }

        public SelectFromQuery<IQueryableRow<TResult>, TResult> FromAll()
        {
            return new(_indexedDB);
        }
    }
}

namespace Svelto.ECS.Schema
{
    public static class SelectQueryExtensions
    {
        public static SelectQuery<TResult> Select<TResult>(this IndexedDB indexedDB)
            where TResult : struct, IResultSet
        {
            return new(indexedDB);
        }

        public static SelectFromQuery<TRow, EmptyResultSet> FromAll<TRow>(this IndexedDB indexedDB)
            where TRow : class, IEntityRow
        {
            return new(indexedDB);
        }

        public static SelectFromQuery<TRow, EmptyResultSet> From<TRow>(this IndexedDB indexedDB, IEntityTables<TRow> tables)
            where TRow : class, IEntityRow
        {
            return new(indexedDB, tables);
        }
    }
}