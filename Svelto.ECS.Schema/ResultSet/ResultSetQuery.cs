using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
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

        internal FasterDictionary<int, int> pkToValue = new FasterDictionary<int, int>();
        internal FasterList<IndexerKeyData> indexers = new FasterList<IndexerKeyData>();

        internal static ThreadLocal<Stack<ResultSetQueryConfig>> Pool =
            new ThreadLocal<Stack<ResultSetQueryConfig>>(() => new Stack<ResultSetQueryConfig>());

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

            Pool.Value.Push(config);
        }
    }

    public readonly ref struct SelectQuery<TRow, TResult>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
    {
        internal readonly ResultSetQueryConfig config;

        internal SelectQuery(IndexedDB indexedDB)
        {
            config = ResultSetQueryConfig.Use();
            config.indexedDB = indexedDB;
        }

        public void Dispose()
        {
            ResultSetQueryConfig.Return(config);
        }

        // Select -> From Tables
        public SelectFromQuery<TResult, TTableRow> From<TTableRow>(
                IEntityTables<TTableRow> tables)
            where TTableRow : class, TRow
        {
            return new SelectFromQuery<TResult, TTableRow>(config, tables);
        }

        public SelectFromQuery<TResult, TRow> FromAll() => FromAll<TRow>();

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.FindTables<TTR>());
        /// </summary>
        public SelectFromQuery<TResult, TTableRow> FromAll<TTableRow>()
            where TTableRow : class, TRow
        {
            return new SelectFromQuery<TResult, TTableRow>(
                config, config.indexedDB.FindTables<TTableRow>());
        }
    }

    public readonly ref struct SelectFromQuery<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        internal readonly ResultSetQueryConfig config;
        internal readonly IEntityTables<TRow> table;

        internal SelectFromQuery(ResultSetQueryConfig config, IEntityTables<TRow> table)
        {
            this.config = config;
            this.table = table;
        }

        public void Dispose()
        {
            ResultSetQueryConfig.Return(config);
        }

        public TablesQueryEnumerable<TResult, TRow> Entities()
        {
            if (config.indexers.count == 0)
                return new TablesQueryEnumerable<TResult, TRow>(config, table);
            else
                return new TablesIndexQueryEnumerable<TResult, TRow>(config, table);
        }

        public SelectFromQuery<TResult, TRow> Where<TIndexQuery>(TIndexQuery query)
            where TIndexQuery : IIndexQuery<TRow>
        {
            query.Apply(config);
            return this;
        }

        public TablesQueryEnumerable<TResult, TRow>.RefIterator GetEnumerator()
            => Entities().GetEnumerator();
    }

    // public readonly ref struct SelectFromTableWhereQuery<TResult, TRow, TIndex>
    //     where TResult : struct, IResultSet
    //     where TRow : class, IEntityRow
    //     where TIndex : IIndexQuery
    // {
    //     internal readonly IndexedDB Item1;
    //     internal readonly IEntityTable<TRow> Item3;
    //     internal readonly TIndex Item4;

    //     internal SelectFromTableWhereQuery(IndexedDB indexedDB, IEntityTable<TRow> table, TIndex indexQuery)
    //     {
    //         Item1 = indexedDB;
    //         Item3 = table;
    //         Item4 = indexQuery;
    //     }

    //     // Select -> From Table -> Where -> Indices
    //     public IndexedIndices Indices()
    //     {
    //         var keyData = Item4.GetIndexerKeyData(Item1);
    //         var group = Item3.ExclusiveGroup;

    //         if (keyData.groups == null || !group.IsEnabled() ||
    //             !keyData.groups.TryGetValue(group, out var groupData))
    //         {
    //             return default;
    //         }

    //         return new IndexedIndices(groupData.filter.filteredIndices);
    //     }

    //     public IndexedQueryResult<TResult, TRow> Entities()
    //     {
    //         ResultSetHelper<TResult>.Assign(out var result, Item1.entitiesDB, Item3.ExclusiveGroup);
    //         return new IndexedQueryResult<TResult, TRow>(result, Indices(), Item3);
    //     }
    // }

    // public readonly ref struct SelectFromTablesWhereQuery<TResult, TRow, TIndex>
    //     where TResult : struct, IResultSet
    //     where TRow : class, IEntityRow
    //     where TIndex : IIndexQuery
    // {
    //     internal readonly IndexedDB Item1;
    //     internal readonly IEntityTables<TRow> Item3;
    //     internal readonly TIndex Item4;

    //     internal SelectFromTablesWhereQuery(IndexedDB indexedDB, IEntityTables<TRow> tables, TIndex indexQuery)
    //     {
    //         Item1 = indexedDB;
    //         Item3 = tables;
    //         Item4 = indexQuery;
    //     }

    //     public TablesIndexQueryEnumerable<TResult, TRow> Entities()
    //     {
    //         return new TablesIndexQueryEnumerable<TResult, TRow>(
    //             Item1, Item3, Item4.GetIndexerKeyData(Item1).groups);
    //     }

    //     public TablesIndexQueryEnumerable<TResult, TRow>.RefIterator GetEnumerator()
    //         => Entities().GetEnumerator();
    // }







}

namespace Svelto.ECS.Schema
{
    public static class RowQueryExtensions
    {
        public static SelectQuery<IQueryableRow<TResult>, TResult> Select<TResult>(this IndexedDB indexedDB)
            where TResult : struct, IResultSet
        {
            return new SelectQuery<IQueryableRow<TResult>, TResult>(indexedDB);
        }
    }
}