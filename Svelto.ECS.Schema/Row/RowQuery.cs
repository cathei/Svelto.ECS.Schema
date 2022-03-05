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

        public SelectFromTablesQuery<TRow, TRow> All() => All<TRow>();

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.Select<TTR>().Tables());
        /// </summary>
        public SelectFromTablesQuery<TRow, TTableRow> All<TTableRow>()
            where TTableRow : class, TRow
        {
            return new SelectFromTablesQuery<TRow, TTableRow>(Item1, Item1.FindTables<TTableRow>());
        }
    }

    public readonly ref struct SelectFromTableQuery<TRow, TTableRow>
        where TRow : class, IEntityRow
        where TTableRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTable<TTableRow> Item3;

        internal SelectFromTableQuery(IndexedDB indexedDB, IEntityTable<TTableRow> table)
        {
            Item1 = indexedDB;
            Item3 = table;
        }

        public static implicit operator SelectFromTableQuery<TRow, TTableRow>(
            (IndexedDB, TRow, IEntityTable<TTableRow>) tuple)
        {
            return new SelectFromTableQuery<TRow, TTableRow>(
                tuple.Item1, tuple.Item3
            );
        }

        // Some magic here
        public void Deconstruct(out IndexedDB indexedDB, out TRow row, out IEntityTable<TTableRow> table)
        {
            indexedDB = Item1;
            row = null;
            table = Item3;
        }
    }

    public readonly ref struct SelectFromTablesQuery<TRow, TTableRow>
        where TRow : class, IEntityRow
        where TTableRow : class, IEntityRow
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTables<TTableRow> Item3;

        internal SelectFromTablesQuery(IndexedDB indexedDB, IEntityTables<TTableRow> tables)
        {
            Item1 = indexedDB;
            Item3 = tables;
        }
    }

    public readonly ref struct SelectFromTableWhereQuery<TRow, TTableRow, TIndexQuery>
        where TRow : class, IEntityRow
        where TTableRow : class, IEntityRow
        where TIndexQuery : IIndexQuery
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTable<TTableRow> Item3;
        internal readonly TIndexQuery Item4;

        public static implicit operator SelectFromTableWhereQuery<TRow, TTableRow, TIndexQuery>(
            (IndexedDB, TRow, IEntityTable<TTableRow>, TIndexQuery) tuple)
        {
            return new SelectFromTableWhereQuery<TRow, TTableRow, TIndexQuery>(
                tuple.Item1, tuple.Item3, tuple.Item4
            );
        }

        // Some magic here
        public void Deconstruct(out IndexedDB indexedDB, out TRow row, out IEntityTable<TTableRow> table, out TIndexQuery index)
        {
            indexedDB = Item1;
            row = null;
            table = Item3;
            index = Item4;
        }

        internal SelectFromTableWhereQuery(IndexedDB indexedDB, IEntityTable<TTableRow> table, TIndexQuery index)
        {
            Item1 = indexedDB;
            Item3 = table;
            Item4 = index;
        }
    }

    public readonly ref struct SelectFromTablesWhereQuery<TRow, TTableRow, TIndexQuery>
        where TRow : class, IEntityRow
        where TTableRow : class, IEntityRow
        where TIndexQuery : IIndexQuery
    {
        internal readonly IndexedDB Item1;
        internal readonly IEntityTables<TTableRow> Item3;
        internal readonly TIndexQuery Item4;

        internal SelectFromTablesWhereQuery(IndexedDB indexedDB, IEntityTables<TTableRow> tables, TIndexQuery index)
        {
            Item1 = indexedDB;
            Item3 = tables;
            Item4 = index;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public static partial class RowQueryExtensions
    {
        // query entrypoint Select -> (From ->) (Where ->) Entities
        // query entrypoint Select -> From Table -> Where -> Indices
        // query entrypoint Select -> Groups
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
        public static SelectFromTableQuery<TR, TTR> From<TR, TTR>(
                this in SelectQuery<TR> query, IEntityTable<TTR> table)
            where TR : class, IEntityRow
            where TTR : class, TR
        {
            return new SelectFromTableQuery<TR, TTR>(query.Item1, table);
        }

        // Select -> From Tables
        public static SelectFromTablesQuery<TR, TTR> From<TR, TTR>(
                this in SelectQuery<TR> query, IEntityTables<TTR> tables)
            where TR : class, IEntityRow
            where TTR : class, TR
        {
            return new SelectFromTablesQuery<TR, TTR>(query.Item1, tables);
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static SelectFromTableWhereQuery<TR, TTR, IndexQuery<TIR, TIK>>
                Where<TR, TTR, TIR, TIK>(this in SelectFromTableQuery<TR, TTR> query,
                    IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IKeyEquatable<TIK>
        {
            return new SelectFromTableWhereQuery<TR, TTR, IndexQuery<TIR, TIK>>(
                query.Item1, query.Item3, index.Query(key));
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static SelectFromTablesWhereQuery<TR, TTR, IndexQuery<TIR, TIK>>
                Where<TR, TTR, TIR, TIK>(this in SelectFromTablesQuery<TR, TTR> query,
                    IIndexQueryable<TIR, TIK> index, TIK key)
            where TR : class, IEntityRow
            where TTR : class, TR, TIR
            where TIR : class, IEntityRow
            where TIK : unmanaged, IKeyEquatable<TIK>
        {
            return new SelectFromTablesWhereQuery<TR, TTR, IndexQuery<TIR, TIK>>(
                query.Item1, query.Item3, index.Query(key));
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static SelectFromTableWhereQuery<TR, TTR, MemoBase<TMR, TMC>>
                Where<TR, TTR, TMR, TMC>(this in SelectFromTableQuery<TR, TTR> query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TTR : class, TR, TMR
            where TMR : class, ISelectorRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return new SelectFromTableWhereQuery<TR, TTR, MemoBase<TMR, TMC>>(
                query.Item1, query.Item3, memo);
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static SelectFromTablesWhereQuery<TR, TTR, MemoBase<TMR, TMC>>
                Where<TR, TTR, TMR, TMC>(this in SelectFromTablesQuery<TR, TTR> query, MemoBase<TMR, TMC> memo)
            where TR : class, IEntityRow
            where TTR : class, TR, TMR
            where TMR : class, ISelectorRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return new SelectFromTablesWhereQuery<TR, TTR, MemoBase<TMR, TMC>>(
                query.Item1, query.Item3, memo);
        }

        // Select -> From Table -> Where -> Indices
        internal static IndexedIndices Indices<TR, TTR, TI>(
                this in SelectFromTableWhereQuery<TR, TTR, TI> query)
            where TR : class, IEntityRow, ISelectorRow<EGIDComponent>
            where TTR : class, IEntityRow
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



        // Select -> From Table -> Entities
        // public static EntityCollection<T1> Entities<TR, T1>(
        //         this (IndexedDB, ISelectorRow<T1>, IEntityTable<TR>) query)
        //     where TR : class, ISelectorRow<T1>
        //         where T1 : struct, IEntityComponent
        // {
        //     return query.Item1.entitiesDB.QueryEntities<T1>(query.Item3.ExclusiveGroup);
        // }

        public interface I {}

        public class A : I {}
        public class B : A {}

        public interface ICov<out I> {}

        public class Container<T> {}

        public static void CanICallThis(this Container<ICov<A>> haha)
        {

        }
        public static void CanICallThisAsWell(this Container<A> haha)
        {

        }

        interface SelectorRow : ISelectorRow<EGIDComponent>();


        public static void Test()
        {

            IndexedDB db = null;
            IEntityTable<SelectorRow> x = null;
            var (a, b, c) = db.Select<SelectorRow>().From(x);
            (a, b, c).Entities();

        }

        public static EntityCollection<T1> Entities<TR, T1>(
                this in SelectFromTableQuery<ISelectorRow<T1>, IEntityTable<TR>> query)
            where TR : class, ISelectorRow<T1>
                where T1 : struct, IEntityComponent
        {{
            return query.Item1.entitiesDB.QueryEntities<T1>(query.Item3.ExclusiveGroup);
        }}

    }
}