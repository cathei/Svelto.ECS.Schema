 // Auto-generated code
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static partial class RowQueryExtensions
    {

        // Select -> Groups
        public static LocalFasterReadOnlyList<ExclusiveGroupStruct> Groups<T1>(
                this (IndexedDB, IEntityRow<T1>) query)
                where T1 : struct, IEntityComponent
        {
            // TODO this ultimately should return Tables<TR>
            return query.Item1.entitiesDB.FindGroups<T1>();
        }

        // Select -> From Table -> Entities
        public static EntityCollection<T1> Entities<TR, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTable<TR>) query)
            where TR : IEntityRow<T1>
                where T1 : struct, IEntityComponent
        {
            return query.Item1.entitiesDB.QueryEntities<T1>(query.Item3.ExclusiveGroup);
        }

        // Select -> From Tables -> Entities
        public static TablesEnumerable<TR, T1> Entities<TR, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTables<TR>) query)
            where TR : IEntityRow<T1>
                where T1 : struct, IEntityComponent
        {
            return new TablesEnumerable<TR, T1>(query.Item1, query.Item3);
        }

        // Select -> From Table -> Where -> Entities
        public static IndexQueryTuple<T1> Entities<TR, TI, TIR, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTable<TR>, TI, TIR) query)
            where TR : IEntityRow<T1>
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, TIR, T1> Entities<TR, TI, TIR, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTables<TR>, TI, TIR) query)
            where TR : IEntityRow<T1>, TIR
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, TIR, T1>(
                query.Item1, query.Item3, query.Item4.GetIndexedKeyData(query.Item1).groups);
        }

        // Select -> Groups
        public static LocalFasterReadOnlyList<ExclusiveGroupStruct> Groups<T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>) query)
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            // TODO this ultimately should return Tables<TR>
            return query.Item1.entitiesDB.FindGroups<T1, T2>();
        }

        // Select -> From Table -> Entities
        public static EntityCollection<T1, T2> Entities<TR, T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>, IEntityTable<TR>) query)
            where TR : IEntityRow<T1, T2>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            return query.Item1.entitiesDB.QueryEntities<T1, T2>(query.Item3.ExclusiveGroup);
        }

        // Select -> From Tables -> Entities
        public static TablesEnumerable<TR, T1, T2> Entities<TR, T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>, IEntityTables<TR>) query)
            where TR : IEntityRow<T1, T2>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            return new TablesEnumerable<TR, T1, T2>(query.Item1, query.Item3);
        }

        // Select -> From Table -> Where -> Entities
        public static IndexQueryTuple<T1, T2> Entities<TR, TI, TIR, T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>, IEntityTable<TR>, TI, TIR) query)
            where TR : IEntityRow<T1, T2>
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, T2>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, TIR, T1, T2> Entities<TR, TI, TIR, T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>, IEntityTables<TR>, TI, TIR) query)
            where TR : IEntityRow<T1, T2>, TIR
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, TIR, T1, T2>(
                query.Item1, query.Item3, query.Item4.GetIndexedKeyData(query.Item1).groups);
        }

        // Select -> Groups
        public static LocalFasterReadOnlyList<ExclusiveGroupStruct> Groups<T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>) query)
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            // TODO this ultimately should return Tables<TR>
            return query.Item1.entitiesDB.FindGroups<T1, T2, T3>();
        }

        // Select -> From Table -> Entities
        public static EntityCollection<T1, T2, T3> Entities<TR, T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTable<TR>) query)
            where TR : IEntityRow<T1, T2, T3>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            return query.Item1.entitiesDB.QueryEntities<T1, T2, T3>(query.Item3.ExclusiveGroup);
        }

        // Select -> From Tables -> Entities
        public static TablesEnumerable<TR, T1, T2, T3> Entities<TR, T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTables<TR>) query)
            where TR : IEntityRow<T1, T2, T3>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            return new TablesEnumerable<TR, T1, T2, T3>(query.Item1, query.Item3);
        }

        // Select -> From Table -> Where -> Entities
        public static IndexQueryTuple<T1, T2, T3> Entities<TR, TI, TIR, T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTable<TR>, TI, TIR) query)
            where TR : IEntityRow<T1, T2, T3>
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, T2, T3>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, TIR, T1, T2, T3> Entities<TR, TI, TIR, T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTables<TR>, TI, TIR) query)
            where TR : IEntityRow<T1, T2, T3>, TIR
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, TIR, T1, T2, T3>(
                query.Item1, query.Item3, query.Item4.GetIndexedKeyData(query.Item1).groups);
        }

        // Select -> Groups
        public static LocalFasterReadOnlyList<ExclusiveGroupStruct> Groups<T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>) query)
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            // TODO this ultimately should return Tables<TR>
            return query.Item1.entitiesDB.FindGroups<T1, T2, T3, T4>();
        }

        // Select -> From Table -> Entities
        public static EntityCollection<T1, T2, T3, T4> Entities<TR, T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>, IEntityTable<TR>) query)
            where TR : IEntityRow<T1, T2, T3, T4>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            return query.Item1.entitiesDB.QueryEntities<T1, T2, T3, T4>(query.Item3.ExclusiveGroup);
        }

        // Select -> From Tables -> Entities
        public static TablesEnumerable<TR, T1, T2, T3, T4> Entities<TR, T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>, IEntityTables<TR>) query)
            where TR : IEntityRow<T1, T2, T3, T4>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            return new TablesEnumerable<TR, T1, T2, T3, T4>(query.Item1, query.Item3);
        }

        // Select -> From Table -> Where -> Entities
        public static IndexQueryTuple<T1, T2, T3, T4> Entities<TR, TI, TIR, T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>, IEntityTable<TR>, TI, TIR) query)
            where TR : IEntityRow<T1, T2, T3, T4>
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, T2, T3, T4>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, TIR, T1, T2, T3, T4> Entities<TR, TI, TIR, T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>, IEntityTables<TR>, TI, TIR) query)
            where TR : IEntityRow<T1, T2, T3, T4>, TIR
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, TIR, T1, T2, T3, T4>(
                query.Item1, query.Item3, query.Item4.GetIndexedKeyData(query.Item1).groups);
        }

    }
}