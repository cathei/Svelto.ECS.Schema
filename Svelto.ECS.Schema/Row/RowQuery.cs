using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct SelectQuery<TRow, TResult>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
    {
        internal readonly IndexedDB Item1;

        internal SelectQuery(IndexedDB indexedDB)
        {
            Item1 = indexedDB;
        }

        // Select -> From Table
        public SelectFromTableQuery<TRow, TResult, TTableRow> From<TTableRow>(
                IEntityTable<TTableRow> table)
            where TTableRow : class, TRow
        {
            return new SelectFromTableQuery<TRow, TResult, TTableRow>(Item1, table);
        }

        // Select -> From Tables
        public SelectFromTablesQuery<TRow, TResult, TTableRow> From<TTableRow>(
                IEntityTables<TTableRow> tables)
            where TTableRow : class, TRow
        {
            return new SelectFromTablesQuery<TRow, TResult, TTableRow>(Item1, tables);
        }

        public SelectFromTablesQuery<TRow, TResult, TRow> FromAll() => FromAll<TRow>();

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.FindTables<TTR>());
        /// </summary>
        public SelectFromTablesQuery<TRow, TResult, TTableRow> FromAll<TTableRow>()
            where TTableRow : class, TRow
        {
            return new SelectFromTablesQuery<TRow, TResult, TTableRow>(Item1, Item1.FindTables<TTableRow>());
        }
    }

    public readonly ref struct SelectFromTableQuery<TRow, TResult, TTableRow>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TTableRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTable<TTableRow> Item3;

        internal SelectFromTableQuery(IndexedDB indexedDB, IEntityTable<TTableRow> table)
        {
            Item1 = indexedDB;
            Item3 = table;
        }

        public QueryResult<TResult, TTableRow> Entities()
        {
            TResult result = default;
            result.LoadEntities(Item1.entitiesDB, Item3.ExclusiveGroup);
            return new QueryResult<TResult, TTableRow>(result, Item3);
        }
    }

    public readonly ref struct SelectFromTablesQuery<TRow, TResult, TTableRow>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TTableRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTables<TTableRow> Item3;

        internal SelectFromTablesQuery(IndexedDB indexedDB, IEntityTables<TTableRow> tables)
        {
            Item1 = indexedDB;
            Item3 = tables;
        }

        public TablesQueryEnumerable<TResult, TTableRow> Entities()
        {
            return new TablesQueryEnumerable<TResult, TTableRow>(Item1, Item3);
        }
    }

    public readonly ref struct SelectFromTableWhereQuery<TRow, TResult, TTableRow, TIndexQuery>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TTableRow : class, IEntityRow
        where TIndexQuery : IIndexQuery
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTable<TTableRow> Item3;
        internal readonly TIndexQuery Item4;

        internal SelectFromTableWhereQuery(IndexedDB indexedDB, IEntityTable<TTableRow> table, TIndexQuery indexQuery)
        {
            Item1 = indexedDB;
            Item3 = table;
            Item4 = indexQuery;
        }

        // Select -> From Table -> Where -> Indices
        internal IndexedIndices Indices()
        {
            var keyData = Item4.GetIndexerKeyData(Item1);
            var group = Item3.ExclusiveGroup;

            if (keyData.groups == null || !group.IsEnabled() ||
                !keyData.groups.TryGetValue(group, out var groupData))
            {
                return default;
            }

            return new IndexedIndices(groupData.filter.filteredIndices);
        }

        public IndexedQueryResult<TResult, TTableRow> Entities()
        {
            TResult result = default;
            result.LoadEntities(Item1.entitiesDB, Item3.ExclusiveGroup);
            return new IndexedQueryResult<TResult, TTableRow>(result, Indices(), Item3);
        }
    }

    public readonly ref struct SelectFromTablesWhereQuery<TRow, TResult, TTableRow, TIndexQuery>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
        where TTableRow : class, IEntityRow
        where TIndexQuery : IIndexQuery
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTables<TTableRow> Item3;
        internal readonly TIndexQuery Item4;

        internal SelectFromTablesWhereQuery(IndexedDB indexedDB, IEntityTables<TTableRow> tables, TIndexQuery indexQuery)
        {
            Item1 = indexedDB;
            Item3 = tables;
            Item4 = indexQuery;
        }

        public TablesIndexQueryEnumerable<TResult, TTableRow> Entities()
        {
            return new TablesIndexQueryEnumerable<TResult, TTableRow>(
                Item1, Item3, Item4.GetIndexerKeyData(Item1).groups);
        }
    }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// We need some notes about why this extensions use tuples
    /// Tuple supports Covariance by deconstructive assignment,
    /// This is normally only possible with interfaces.
    /// so we can chain extension methods properly with Value type query.
    /// </summary>
    public static partial class RowQueryExtensions
    {
        internal static IndexQuery IndexQuery<TRow, TComponent, TKey>(
                this IndexedDB indexedDB, IIndexQueryable<TRow, TComponent> index, TKey key)
            where TRow : class, IReactiveRow<TComponent>
            where TComponent : unmanaged, IIndexableComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            var indexerID = index.IndexerID;
            if (!indexedDB.indexers.ContainsKey(indexerID))
                return default;

            var indexerData = (IndexerData<TKey>)indexedDB.indexers[indexerID];
            indexerData.TryGetValue(key, out var result);
            return new IndexQuery(result);
        }


        // query entrypoint Select -> (From ->) (Where ->) Entities
        // query entrypoint Select -> From Table -> Where -> Indices
        // query entrypoint Select -> Tables
        public static SelectQuery<IQueryableRow<TResult>, TResult> Select<TResult>(this IndexedDB indexedDB)
            where TResult : struct, IResultSet
        {
            return new SelectQuery<IQueryableRow<TResult>, TResult>(indexedDB);
        }

        // Select -> From Table -> Where
        // Where methods are extensions because there's table row restraints
        // Table Row must implement both Selector Row and Index Row
        public static SelectFromTableWhereQuery<TR, TRS, TTR, IndexQuery<TIR, TIK>> Where<TR, TRS, TTR, TIR, TIK>(
                this in SelectFromTableQuery<TR, TRS, TTR> query, IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TRS : struct, IResultSet
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IEquatable<TIK>
        {
            return new SelectFromTableWhereQuery<TR, TRS, TTR, IndexQuery<TIR, TIK>>(query.Item1, query.Item3, index.Query(key));
        }

        // Select -> From Tables -> Where
        // Where methods are extensions because there's table row restraints
        // Tables Row must implement both Selector Row and Index Row
        public static SelectFromTablesWhereQuery<TR, TRS, TTR, IndexQuery<TIR, TIK>> Where<TR, TRS, TTR, TIR, TIK>(
                this in SelectFromTablesQuery<TR, TRS, TTR> query, IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TRS : struct, IResultSet
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IEquatable<TIK>
        {
            return new SelectFromTablesWhereQuery<TR, TRS, TTR, IndexQuery<TIR, TIK>>(query.Item1, query.Item3, index.Query(key));
        }

        // Select -> From Table -> Where
        // Where methods are extensions because there's table row restraints
        // Table Row must implement both Selector Row and Index Row
        public static SelectFromTableWhereQuery<TR, TRS, TTR, MemoBase<TMR, TMC>> Where<TR, TRS, TTR, TMR, TMC>(
                this in SelectFromTableQuery<TR, TRS, TTR> query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TRS : struct, IResultSet
            where TTR : class, TR, TMR
            where TMR : class, IIndexableRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return new SelectFromTableWhereQuery<TR, TRS, TTR, MemoBase<TMR, TMC>>(query.Item1, query.Item3, memo);
        }

        // Select -> From Tables -> Where
        // Where methods are extensions because there's table row restraints
        // Tables Row must implement both Selector Row and Index Row
        public static SelectFromTablesWhereQuery<TR, TRS, TTR, MemoBase<TMR, TMC>> Where<TR, TRS, TTR, TMR, TMC>(
                this in SelectFromTablesQuery<TR, TRS, TTR> query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TRS : struct, IResultSet
            where TTR : class, TR, TMR
            where TMR : class, IIndexableRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return new SelectFromTablesWhereQuery<TR, TRS, TTR, MemoBase<TMR, TMC>>(query.Item1, query.Item3, memo);
        }
    }
}