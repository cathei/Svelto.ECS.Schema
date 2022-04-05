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
    public struct Empty
    {
        internal static readonly SelectFromQueryDelegate<Empty> Selector =
            (EntitiesDB entitiesDB, in ExclusiveGroupStruct group) => default;
    }

    public readonly ref struct SelectQuery<TResult>
        where TResult : struct, IResultSet
    {
        internal readonly IndexedDB _indexedDB;

        public SelectQuery(IndexedDB indexedDB)
        {
            _indexedDB = indexedDB;
        }

        private static TResult QueryEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct group)
        {
            ResultSetHelper<TResult>.Assign(out var result, entitiesDB, group);
            return result;
        }

        private static readonly SelectFromQueryDelegate<TResult> Selector = QueryEntities;

        public SelectFromQuery<TRow, TResult> From<TRow>()
            where TRow : class, IQueryableRow<TResult>
        {
            return new(_indexedDB, Selector);
        }

        public SelectFromQuery<TRow, TResult> From<TRow>(IEntityTables<TRow> tables)
            where TRow : class, IQueryableRow<TResult>
        {
            return new(_indexedDB, tables, Selector);
        }

        public SelectFromQuery<IQueryableRow<TResult>, TResult> FromAll()
        {
            return new(_indexedDB, Selector);
        }
    }

    // public readonly ref struct SelectQuery<TResult1, TResult2>
    //     where TResult1 : struct, IResultSet
    //     where TResult2 : struct, IResultSet
    // {
    //     internal readonly IndexedDB _indexedDB;

    //     public SelectQuery(IndexedDB indexedDB)
    //     {
    //         _indexedDB = indexedDB;
    //     }

    //     private static (TResult1, TResult2) QueryEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct group)
    //     {
    //         ResultSetHelper<TResult1>.Assign(out var result1, entitiesDB, group);
    //         ResultSetHelper<TResult2>.Assign(out var result2, entitiesDB, group);
    //         return (result1, result2);
    //     }

    //     private static readonly SelectFromQueryDelegate<(TResult1, TResult2)> Selector = QueryEntities;

    //     public SelectFromQuery<TRow, (TResult1, TResult2)> From<TRow>()
    //         where TRow : class, IQueryableRow<TResult1>, IQueryableRow<TResult2>
    //     {
    //         return new(_indexedDB, Selector);
    //     }

    //     public SelectFromQuery<TRow, (TResult1, TResult2)> From<TRow>(IEntityTables<TRow> tables)
    //         where TRow : class, IQueryableRow<TResult1>, IQueryableRow<TResult2>
    //     {
    //         return new(_indexedDB, tables, Selector);
    //     }
    // }
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

        // public static SelectQuery<TResult1, TResult2> Select<TResult1, TResult2>(this IndexedDB indexedDB)
        //     where TResult1 : struct, IResultSet
        //     where TResult2 : struct, IResultSet
        // {
        //     return new(indexedDB);
        // }

        public static SelectFromQuery<TRow, Empty> From<TRow>(this IndexedDB indexedDB)
            where TRow : class, IEntityRow
        {
            return new(indexedDB, Empty.Selector);
        }

        public static SelectFromQuery<TRow, Empty> From<TRow>(this IndexedDB indexedDB, IEntityTables<TRow> tables)
            where TRow : class, IEntityRow
        {
            return new(indexedDB, tables, Empty.Selector);
        }
    }
}