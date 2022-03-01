using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IEntityIndexQueriable<TK, TR>
        where TK : unmanaged
        where TR : IEntityRow
    {
        EntityIndexQuery<TK, TR> Query(TK key);
    }

    public readonly struct EntityIndexQuery<TK, TR> : IEntityIndexQuery
        where TK : unmanaged
        where TR : IEntityRow
    {
        private readonly int _indexerId;
        private readonly TK _key;

        internal EntityIndexQuery(int indexerId, in TK key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        public IndexedKeyData GetIndexedKeyData(IndexedDB indexedDB)
        {
            // TODO: find with row
            return new IndexedKeyData();
        }
    }

    public static class RowQueryExtensions
    {
        // query entrypoint Select -> (From ->) (Where ->) Entities
        // query entrypoint Select -> From Table -> Where -> Indices
        // query entrypoint Select -> Groups
        public static (IndexedDB, TR) Select<TR>(this IndexedDB indexedDB)
            where TR : IEntityRow
        {
            return (indexedDB, default);
        }

        // Select -> From Table
        public static (IndexedDB, TR, IEntityTable<TTR>) From<TR, TTR>(
                this (IndexedDB, TR) query, IEntityTable<TTR> table)
            where TR : IEntityRow
            where TTR : TR
        {
            return (query.Item1, query.Item2, table);
        }

        // Select -> From Tables
        public static (IndexedDB, TR, IEntityTables<TTR>) From<TR, TTR>(
                this (IndexedDB, TR) query, IEntityTables<TTR> tables)
            where TR : IEntityRow
            where TTR : TR
        {
            return (query.Item1, query.Item2, tables);
        }

        // Select -> Where
        // Result will be from all groups so no need to check for query type
        public static (IndexedDB, TR, EntityIndexQuery<TIK, TIR>)
                Where<TR, TIK, TIR>(this (IndexedDB, TR) query,
                    IEntityIndexQueriable<TIK, TIR> index, TIK key)
            where TR : IEntityRow
            where TIK : unmanaged
            where TIR : IEntityRow
        {
            return (query.Item1, query.Item2, index.Query(key));
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTable<TTR>, EntityIndexQuery<TIK, TIR>)
                Where<TR, TTR, TIK, TIR>(this (IndexedDB, TR, IEntityTable<TTR>) query,
                    IEntityIndexQueriable<TIK, TIR> index, TIK key)
            where TR : IEntityRow
            where TTR : TR, TIR
            where TIK : unmanaged
            where TIR : IEntityRow
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTables<TTR>, EntityIndexQuery<TIK, TIR>)
                Where<TR, TTR, TIK, TIR>(this (IndexedDB, TR, IEntityTables<TTR>) query,
                    IEntityIndexQueriable<TIK, TIR> index, TIK key)
            where TR : IEntityRow
            where TTR : TR, TIR
            where TIK : unmanaged
            where TIR : IEntityRow
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Table -> Where -> Indices
        internal static IndexedIndices Indices<TR, TI>(
                this (IndexedDB, TR, IEntityTable, TI) query)
            where TR : IEntityRow
            where TI : IEntityIndexQuery
        {
            var keyData = query.Item4.GetIndexedKeyData(query.Item1);
            var group = query.Item3.ExclusiveGroup;

            if (keyData.groups == null || !group.IsEnabled() ||
                !keyData.groups.TryGetValue(group, out var groupData))
            {
                return default;
            }

            return new IndexedIndices(groupData.filter.filteredIndices);
        }

        ////// TO BE GENERATED

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

        // Select -> Entities
        public static GroupsEnumerable<T1, T2, T3> Entities<T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>) query)
            where T1 : struct, IEntityComponent
            where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent
        {
            // TODO : is it possible to have findgroups cache inside of IndexedDB?
            // Is it better to cache tables or just rely on FindGroups?
            var groups = query.Item1.entitiesDB.FindGroups<T1, T2, T3>();
            return query.Item1.entitiesDB.QueryEntities<T1, T2, T3>(groups);
        }

        // Select -> From Table -> Entities
        public static EntityCollection<T1, T2, T3> Entities<T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTable) query)
            where T1 : struct, IEntityComponent
            where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent
        {
            return query.Item1.entitiesDB.QueryEntities<T1, T2, T3>(query.Item3.ExclusiveGroup);
        }

        // Select -> From Tables -> Entities
        public static GroupsEnumerable<T1, T2, T3> Entities<T1, T2, T3>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTables) query)
            where T1 : struct, IEntityComponent
            where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent
        {
            return query.Item1.entitiesDB.QueryEntities<T1, T2, T3>(query.Item3.ExclusiveGroups);
        }

        // Select -> Where -> Entities
        public static IndexQueryEnumerable<T1, T2, T3> Entities<T1, T2, T3, TI>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, TI) query)
            where T1 : struct, IEntityComponent
            where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent
            where TI : struct, IEntityIndexQuery
        {
            return new IndexQueryEnumerable<T1, T2, T3>(
                query.Item1, query.Item3.GetIndexedKeyData(query.Item1).groups);
        }

        // Select -> From Table -> Where -> Entities
        public static IndexQueryTuple<T1, T2, T3> Entities<T1, T2, T3, TI>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTable, TI) query)
            where T1 : struct, IEntityComponent
            where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent
            where TI : struct, IEntityIndexQuery
        {
            return new IndexQueryTuple<T1, T2, T3>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryGroupsEnumerable<T1, T2, T3> Entities<T1, T2, T3, TI>(
                this (IndexedDB, IEntityRow<T1, T2, T3>, IEntityTables, TI) query)
            where T1 : struct, IEntityComponent
            where T2 : struct, IEntityComponent
            where T3 : struct, IEntityComponent
            where TI : struct, IEntityIndexQuery
        {
            var groups = query.Item3.ExclusiveGroups;
            return new IndexQueryGroupsEnumerable<T1, T2, T3>(
                query.Item1, query.Item4.GetIndexedKeyData(query.Item1).groups, groups);
        }
    }
}