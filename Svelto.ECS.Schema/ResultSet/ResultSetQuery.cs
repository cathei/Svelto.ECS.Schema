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
        internal readonly IEntityTables<TRow> tables;

        internal FromRowQuery(IndexedDB indexedDB)
        {
            config = ResultSetQueryConfig.Use();
            config.indexedDB = indexedDB;
            this.tables = indexedDB.FindTables<TRow>();
        }

        internal FromRowQuery(IndexedDB indexedDB, IEntityTables<TRow> tables)
        {
            config = ResultSetQueryConfig.Use();
            config.indexedDB = indexedDB;
            this.tables = tables;
        }

        public FromRowQuery<TRow> Where<T>(T query)
            where T : IIndexQuery<TRow>
        {
            if (config.temporaryGroups.count != 0)
                throw new ECSException("Query is already in use, cannot add Where condition");

            query.Apply(config);
            return this;
        }

        public FromRowQueryEnumerator<TRow> GetEnumerator()
        {
            Build();
            return new(config);
        }

        internal void Build()
        {
            if (config.temporaryGroups.count != 0)
                return;

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

    public readonly ref struct FromGroupQuery<TRow>
        where TRow : class, IEntityRow
    {
        internal readonly ResultSetQueryConfig config;

        public readonly ExclusiveGroupStruct group;
        public readonly MultiIndexedIndices indices;

        public FromGroupQuery(ResultSetQueryConfig config,
            in ExclusiveGroupStruct group, in MultiIndexedIndices indices)
        {
            this.config = config;
            this.group = group;
            this.indices = indices;
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

        public static FromRowQuery<TRow> From<TRow>(this IndexedDB indexedDB, IEntityTables<TRow> table)
            where TRow : class, IEntityRow
        {
            return new(indexedDB, table);
        }

        public static void Select<TRow, TResult>(
                this FromGroupQuery<TRow> query, out TResult result)
            where TRow : class, IQueryableRow<TResult>
            where TResult : struct, IResultSet
        {
            ResultSetHelper<TResult>.Assign(out result, query.config.indexedDB.entitiesDB, query.group);
        }
    }

    public static class FromResultSetQueryExtensions
    {
        public static FromRowQuery<IQueryableRow<TResult>> From<TResult>(this IndexedDB indexedDB)
            where TResult : struct, IResultSet
        {
            return new(indexedDB);
        }

        public static void Select<TResult>(
                this FromGroupQuery<IQueryableRow<TResult>> query, out TResult result)
            where TResult : struct, IResultSet
        {
            ResultSetHelper<TResult>.Assign(out result, query.config.indexedDB.entitiesDB, query.group);
        }
    }
}