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
        public SelectFromTableQuery<TResult, TTableRow> From<TTableRow>(
                IEntityTable<TTableRow> table)
            where TTableRow : class, TRow
        {
            return new SelectFromTableQuery<TResult, TTableRow>(Item1, table);
        }

        // Select -> From Tables
        public SelectFromTablesQuery<TResult, TTableRow> From<TTableRow>(
                IEntityTables<TTableRow> tables)
            where TTableRow : class, TRow
        {
            return new SelectFromTablesQuery<TResult, TTableRow>(Item1, tables);
        }

        public SelectFromTablesQuery<TResult, TRow> FromAll() => FromAll<TRow>();

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.FindTables<TTR>());
        /// </summary>
        public SelectFromTablesQuery<TResult, TTableRow> FromAll<TTableRow>()
            where TTableRow : class, TRow
        {
            return new SelectFromTablesQuery<TResult, TTableRow>(Item1, Item1.FindTables<TTableRow>());
        }
    }

    public readonly ref struct SelectFromTableQuery<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTable<TRow> Item3;

        internal SelectFromTableQuery(IndexedDB indexedDB, IEntityTable<TRow> table)
        {
            Item1 = indexedDB;
            Item3 = table;
        }

        public QueryResult<TResult, TRow> Entities()
        {
            ResultSetHelper<TResult>.Assign(out var result, Item1.entitiesDB, Item3.ExclusiveGroup);
            return new QueryResult<TResult, TRow>(result, Item3);
        }

        public SelectFromTableWhereQuery<TResult, TRow, TIndexQuery> Where<TIndexQuery>(TIndexQuery query)
            where TIndexQuery : IIndexQuery<TRow>
        {
            return new SelectFromTableWhereQuery<TResult, TRow, TIndexQuery>(
                Item1, Item3, query);
        }
    }

    public readonly ref struct SelectFromTablesQuery<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTables<TRow> Item3;

        internal SelectFromTablesQuery(IndexedDB indexedDB, IEntityTables<TRow> tables)
        {
            Item1 = indexedDB;
            Item3 = tables;
        }

        public TablesQueryEnumerable<TResult, TRow> Entities()
        {
            return new TablesQueryEnumerable<TResult, TRow>(Item1, Item3);
        }

        public SelectFromTablesWhereQuery<TResult, TRow, TIndexQuery> Where<TIndexQuery>(TIndexQuery query)
            where TIndexQuery : IIndexQuery<TRow>
        {
            return new SelectFromTablesWhereQuery<TResult, TRow, TIndexQuery>(
                Item1, Item3, query);
        }

        public TablesQueryEnumerable<TResult, TRow>.RefIterator GetEnumerator()
            => Entities().GetEnumerator();
    }

    public readonly ref struct SelectFromTableWhereQuery<TResult, TRow, TIndex>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
        where TIndex : IIndexQuery
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTable<TRow> Item3;
        internal readonly TIndex Item4;

        internal SelectFromTableWhereQuery(IndexedDB indexedDB, IEntityTable<TRow> table, TIndex indexQuery)
        {
            Item1 = indexedDB;
            Item3 = table;
            Item4 = indexQuery;
        }

        // Select -> From Table -> Where -> Indices
        public IndexedIndices Indices()
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

        public IndexedQueryResult<TResult, TRow> Entities()
        {
            ResultSetHelper<TResult>.Assign(out var result, Item1.entitiesDB, Item3.ExclusiveGroup);
            return new IndexedQueryResult<TResult, TRow>(result, Indices(), Item3);
        }
    }

    public readonly ref struct SelectFromTablesWhereQuery<TResult, TRow, TIndex>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
        where TIndex : IIndexQuery
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTables<TRow> Item3;
        internal readonly TIndex Item4;

        internal SelectFromTablesWhereQuery(IndexedDB indexedDB, IEntityTables<TRow> tables, TIndex indexQuery)
        {
            Item1 = indexedDB;
            Item3 = tables;
            Item4 = indexQuery;
        }

        public TablesIndexQueryEnumerable<TResult, TRow> Entities()
        {
            return new TablesIndexQueryEnumerable<TResult, TRow>(
                Item1, Item3, Item4.GetIndexerKeyData(Item1).groups);
        }

        public TablesIndexQueryEnumerable<TResult, TRow>.RefIterator GetEnumerator()
            => Entities().GetEnumerator();
    }
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