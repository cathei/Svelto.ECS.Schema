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

        public (IndexedDB, IMagicTuple<TRow, TResult>, IEntityTables<TRow>) FromAll() => FromAll<TRow>();

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.Select<TTR>().Tables());
        /// </summary>
        public (IndexedDB, IMagicTuple<TRow, TResult>, IEntityTables<TTableRow>) FromAll<TTableRow>()
            where TTableRow : class, TRow
        {
            return (Item1, default, Item1.FindTables<TTableRow>());
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
    }

    // magic for type inference ! they can be used for whatever I want :)
    public interface IMagicTuple<out T1, out T2> { }
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
        // query entrypoint Select -> (From ->) (Where ->) Entities
        // query entrypoint Select -> From Table -> Where -> Indices
        // query entrypoint Select -> Tables
        public static SelectQuery<IQueryableRow<TResult>, TResult> Select<TResult>(this IndexedDB indexedDB)
            where TResult : struct, IResultSet
        {
            return new SelectQuery<IQueryableRow<TResult>, TResult>(indexedDB);
        }

        // Select -> From Table
        public static (IndexedDB, IMagicTuple<TR, TRS>, IEntityTable<TTR>) From<TR, TRS, TTR>(
                this in SelectQuery<TR, TRS> query, IEntityTable<TTR> table)
            where TR : class, IEntityRow
            where TRS : struct, IResultSet
            where TTR : class, TR
        {
            return (query.Item1, default, table);
        }

        // Select -> From Tables
        public static (IndexedDB, IMagicTuple<TR, TRS>, IEntityTables<TTR>) From<TR, TRS, TTR>(
                this in SelectQuery<TR, TRS> query, IEntityTables<TTR> tables)
            where TR : class, IEntityRow
            where TRS : struct, IResultSet
            where TTR : class, TR
        {
            return (query.Item1, default, tables);
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, IMagicTuple<TR, TRS>, IEntityTable<TTR>, IndexQuery<TIR, TIK>) Where<TR, TRS, TTR, TIR, TIK>(
                this in (IndexedDB, IMagicTuple<TR, TRS>, IEntityTable<TTR>) query, IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IEquatable<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, IMagicTuple<TR, TRS>, IEntityTables<TTR>, IndexQuery<TIR, TIK>) Where<TR, TRS, TTR, TIR, TIK>(
                this in (IndexedDB, IMagicTuple<TR, TRS>, IEntityTables<TTR>) query, IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IEquatable<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, IMagicTuple<TR, TRS>, IEntityTable<TTR>, MemoBase<TMR, TMC>) Where<TR, TRS, TTR, TMR, TMC>(
                this in (IndexedDB, IMagicTuple<TR, TRS>, IEntityTable<TTR>) query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TTR : class, TR, TMR
            where TMR : class, IIndexableRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return (query.Item1, query.Item2, query.Item3, memo);
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, IMagicTuple<TR, TRS>, IEntityTables<TTR>, MemoBase<TMR, TMC>) Where<TR, TRS, TTR, TMR, TMC>(
                this in (IndexedDB, IMagicTuple<TR, TRS>, IEntityTables<TTR>) query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TTR : class, TR, TMR
            where TMR : class, IIndexableRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return (query.Item1, query.Item2, query.Item3, memo);
        }

        // Select -> From Table -> Where -> Indices
        internal static IndexedIndices Indices<TR, TRS, TI>(
                this in (IndexedDB, IMagicTuple<TR, TRS>, IEntityTable, TI) query)
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