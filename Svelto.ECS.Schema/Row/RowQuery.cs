using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct SelectQuery<TRow>
        where TRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;

        internal SelectQuery(IndexedDB indexedDB)
        {
            Item1 = indexedDB;
        }

        public (IndexedDB, TRow, IEntityTables<TRow>) FromAll() => FromAll<TRow>();

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.Select<TTR>().Tables());
        /// </summary>
        public (IndexedDB, TRow, IEntityTables<TTableRow>) FromAll<TTableRow>()
            where TTableRow : class, TRow
        {
            return (Item1, default, Item1.FindTables<TTableRow>());
        }
    }
}

namespace Svelto.ECS.Schema
{
    public static partial class RowQueryExtensions
    {
        // query entrypoint Select -> (From ->) (Where ->) Entities
        // query entrypoint Select -> From Table -> Where -> Indices
        // query entrypoint Select -> Tables
        public static SelectQuery<TR> Select<TR>(this IndexedDB indexedDB)
            where TR : class, IEntityRow
        {
            return new SelectQuery<TR>(indexedDB);
        }

        /// <summary>
        /// This will find all Tables containing Row type TR
        /// </summary>
        public static IEntityTables<TR> Tables<TR>(this in SelectQuery<TR> query)
            where TR : class, IEntityRow
        {
            return query.Item1.FindTables<TR>();
        }

        // Select -> From Table
        public static (IndexedDB, TR, IEntityTable<TTR>) From<TR, TTR>(
                this in SelectQuery<TR> query, IEntityTable<TTR> table)
            where TR : class, IEntityRow
            where TTR : class, TR
        {
            return (query.Item1, null, table);
        }

        // Select -> From Tables
        public static (IndexedDB, TR, IEntityTables<TTR>) From<TR, TTR>(
                this in SelectQuery<TR> query, IEntityTables<TTR> tables)
            where TR : class, IEntityRow
            where TTR : class, TR
        {
            return (query.Item1, null, tables);
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTable<TTR>, IndexQuery<TIR, TIK>)
                Where<TR, TTR, TIR, TIK>(this (IndexedDB, TR, IEntityTable<TTR>) query,
                    IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IKeyEquatable<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTables<TTR>, IndexQuery<TIR, TIK>)
                Where<TR, TTR, TIR, TIK>(this (IndexedDB, TR, IEntityTables<TTR>) query,
                    IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IKeyEquatable<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTable<TTR>, MemoBase<TMR, TMC>)
                Where<TR, TTR, TMR, TMC>(this (IndexedDB, TR, IEntityTable<TTR>) query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TTR : class, TR, TMR
            where TMR : class, ISelectorRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return (query.Item1, query.Item2, query.Item3, memo);
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTables<TTR>, MemoBase<TMR, TMC>)
                Where<TR, TTR, TMR, TMC>(this (IndexedDB, TR, IEntityTables<TTR>) query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TTR : class, TR, TMR
            where TMR : class, ISelectorRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return (query.Item1, query.Item2, query.Item3, memo);
        }

        // Select -> From Table -> Where -> Indices
        internal static IndexedIndices Indices<TR, TI>(
                this (IndexedDB, TR, IEntityTable, TI) query)
            where TR : class, IEntityRow
            where TI : IIndexQuery
        {
            var keyData = query.Item4.GetIndexerKeyData(query.Item1);
            var group = query.Item3.ExclusiveGroup;

            if (keyData.groups == null || !group.IsEnabled() ||
                !keyData.groups.TryGetValue(group, out var groupData))
            {
                return default;
            }

            return new IndexedIndices(groupData.filter.filteredIndices);
        }
    }
}