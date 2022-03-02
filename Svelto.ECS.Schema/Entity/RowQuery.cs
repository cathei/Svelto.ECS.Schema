using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static partial class RowQueryExtensions
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
        public static (IndexedDB, TR, IndexQuery<TIK, TIC>)
                Where<TR, TIR, TIK, TIC>(this (IndexedDB, TR) query,
                    IIndexQueryable<TIR, TIK, TIC> index, TIK key)
            where TR : IEntityRow
            where TIR : IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            return (query.Item1, query.Item2, index.Query(key));
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTable<TTR>, IndexQuery<TIK, TIC>)
                Where<TR, TTR, TIR, TIK, TIC>(this (IndexedDB, TR, IEntityTable<TTR>) query,
                    IIndexQueryable<TIR, TIK, TIC> index, TIK key)
            where TR : IEntityRow
            where TTR : TR, TIR
            where TIR : IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTables<TTR>, IndexQuery<TIK, TIC>)
                Where<TR, TTR, TIR, TIK, TIC>(this (IndexedDB, TR, IEntityTables<TTR>) query,
                    IIndexQueryable<TIR, TIK, TIC> index, TIK key)
            where TR : IEntityRow
            where TTR : TR, TIR
            where TIR : IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key));
        }

        // Select -> From Table -> Where -> Indices
        internal static IndexedIndices Indices<TR, TI>(
                this (IndexedDB, TR, IEntityTable, TI) query)
            where TR : IEntityRow
            where TI : IIndexQuery
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
    }
}