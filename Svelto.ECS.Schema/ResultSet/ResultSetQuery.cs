using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;
using Svelto.ObjectPool;

namespace Svelto.ECS.Schema.Internal
{
    /// <summary>
    /// this is pooled object and contains all information about current query
    /// disposing any object in select chain will return this object to pool
    /// </summary>
    public sealed class ResultSetQueryConfig
    {
        internal bool isReturned = false;

        internal IndexedDB indexedDB;

        internal FasterDictionary<int, int> pkToValue = new();
        internal FasterList<IndexerKeyData> indexers = new();

        internal SharedSveltoDictionaryNative<ExclusiveGroupStruct, ExclusiveGroupStruct> temporaryGroups = new(0);
        internal NativeDynamicArrayCast<FilterGroup> temporaryFilters = new(NativeDynamicArray.Alloc<FilterGroup>());

        internal static ThreadLocal<Stack<ResultSetQueryConfig>> Pool = new(() => new());

        internal static ResultSetQueryConfig Use()
        {
            if (Pool.Value.TryPop(out var result))
            {
                result.isReturned = false;
                return result;
            }

            return new ResultSetQueryConfig();
        }

        internal static void Return(ResultSetQueryConfig config)
        {
            if (config.isReturned)
                return;

            config.isReturned = true;
            config.indexedDB = null;
            config.pkToValue.FastClear();
            config.indexers.FastClear();

            config.temporaryGroups.FastClear();
            config.temporaryFilters.Clear();

            Pool.Value.Push(config);
        }
    }

    public readonly ref struct FromRowQuery<TRow>
        where TRow : class, IEntityRow
    {
        internal readonly ResultSetQueryConfig config;

        internal FromRowQuery(IndexedDB indexedDB)
        {
            config = ResultSetQueryConfig.Use();
            config.indexedDB = indexedDB;
        }

        public FromRowQuery<TRow> Where<T>(T query)
            where T : IIndexQuery<TRow>
        {
            if (config.temporaryGroups.count != 0)
                throw new ECSException("Query is already in use, cannot add Where condition");

            query.Apply(config);
            return this;
        }

        public FromRowSelectQuery<TRow, T> Select<T>()
            where T : struct, IResultSet
        {
            Build();
            return new(config);
        }

        public void Dispose()
        {
            ResultSetQueryConfig.Return(config);
        }

        private void Build()
        {
            if (config.temporaryGroups.count != 0)
                return;

            var tables = config.indexedDB.FindTables<TRow>();
            int tableCount = tables.Range;

            for (int i = 0; i < tableCount; ++i)
            {
                var table = tables.GetTable(i);

                if (table.PrimaryKeys.count == 0)
                    config.temporaryGroups.Add(table.Group, table.Group);
                else
                    IterateGroup(table);
            }
        }

        private void IterateGroup(IEntityTable<TRow> table, int groupIndex = 0, int depth = 0)
        {
            if (depth >= table.PrimaryKeys.count)
            {
                // table group index 0 is reserved for adding only
                var group = table.Group + (uint)(groupIndex + 1);
                config.temporaryGroups.Add(group, group);
                return;
            }

            var pk = table.PrimaryKeys[depth];

            // mutiply parent index
            groupIndex *= pk.PossibleKeyCount;

            // when sub-index applied
            if (config.pkToValue.TryGetValue(pk.PrimaryKeyID, out var value))
            {
                groupIndex += value;
                IterateGroup(table, groupIndex, depth + 1);
                return;
            }

            // iterate all subgroup
            for (int i = 0; i < pk.PossibleKeyCount; ++i)
            {
                IterateGroup(table, groupIndex + i, depth + 1);
            }
        }
    }

    public readonly ref struct FromRowSelectQuery<TRow, TResult>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
    {
        internal readonly ResultSetQueryConfig config;

        public FromRowSelectQuery(ResultSetQueryConfig config)
        {
            this.config = config;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public static class FromRowQueryExtensions
    {
        public static FromRowQuery<TRow> From<TRow>(this IndexedDB indexedDB)
            where TRow : class, IEntityRow
        {
            return new(indexedDB);
        }

        public static TableQueryEnumerator<TResult> GetEnumerator<TRow, TResult>(
                this FromRowSelectQuery<TRow, TResult> query)
            where TRow : class, IQueryableRow<TResult>
            where TResult : struct, IResultSet
        {
            return new(query.config);
        }
    }

    public static class FromResultSetQueryExtensions
    {
        public static FromRowQuery<IQueryableRow<TResult>> From<TResult>(this IndexedDB indexedDB)
            where TResult : struct, IResultSet
        {
            return new(indexedDB);
        }

        public static FromRowSelectQuery<IQueryableRow<TResult>, TResult> Select<TResult>(
                this FromRowQuery<IQueryableRow<TResult>> query)
            where TResult : struct, IResultSet
        {
            return new(query.config);
        }
    }

    public static class FromGroupQueryExtensions
    {
        // public static FromRowQuery<TRow> From<TRow>(this IndexedDB indexedDB)
        //     where TRow : class, IEntityRow
        // {
        //     return new(indexedDB);
        // }
    }
}