 // Auto-generated code
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static partial class RowQueryExtensions
    {

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
        public static IndexQueryTuple<T1, IndexedIndices> Entities<TR, TI, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTable<TR>, TI) query)
            where TR : IEntityRow<T1>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, IndexedIndices>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, T1> Entities<TR, TI, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTables<TR>, TI) query)
            where TR : IEntityRow<T1>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, T1>(
                query.Item1, query.Item3, query.Item4.GetIndexerKeyData(query.Item1).groups);
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
        public static IndexQueryTuple<T1, T2, IndexedIndices> Entities<TR, TI, T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>, IEntityTable<TR>, TI) query)
            where TR : IEntityRow<T1, T2>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, T2, IndexedIndices>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, T1, T2> Entities<TR, TI, T1, T2>(
                this (IndexedDB, IEntityRow<T1, T2>, IEntityTables<TR>, TI) query)
            where TR : IEntityRow<T1, T2>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, T1, T2>(
                query.Item1, query.Item3, query.Item4.GetIndexerKeyData(query.Item1).groups);
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
        public static IndexQueryTuple<T1, T2, T3, IndexedIndices> Entities<TR, TI, T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTable<TR>, TI) query)
            where TR : IEntityRow<T1, T2, T3>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, T2, T3, IndexedIndices>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, T1, T2, T3> Entities<TR, TI, T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTables<TR>, TI) query)
            where TR : IEntityRow<T1, T2, T3>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, T1, T2, T3>(
                query.Item1, query.Item3, query.Item4.GetIndexerKeyData(query.Item1).groups);
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
        public static IndexQueryTuple<T1, T2, T3, T4, IndexedIndices> Entities<TR, TI, T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>, IEntityTable<TR>, TI) query)
            where TR : IEntityRow<T1, T2, T3, T4>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            return new IndexQueryTuple<T1, T2, T3, T4, IndexedIndices>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TR, T1, T2, T3, T4> Entities<TR, TI, T1, T2, T3, T4>(
                this (IndexedDB, IEntityRow<T1, T2, T3, T4>, IEntityTables<TR>, TI) query)
            where TR : IEntityRow<T1, T2, T3, T4>
            where TI : IIndexQuery
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
        {
            return new IndexQueryEnumerable<TR, T1, T2, T3, T4>(
                query.Item1, query.Item3, query.Item4.GetIndexerKeyData(query.Item1).groups);
        }

    }
}