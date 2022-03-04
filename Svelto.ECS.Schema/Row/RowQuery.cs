using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static partial class RowQueryExtensions
    {
        // query entrypoint Select -> (From ->) (Where ->) Entities
        // query entrypoint Select -> From Table -> Where -> Indices
        // query entrypoint Select -> Groups
        // query entrypoint Select -> Tables
        public static (IndexedDB, TR) Select<TR>(this IndexedDB indexedDB)
            where TR : IEntityRow
        {
            return (indexedDB, default);
        }

        /// <summary>
        /// This will find all Tables containing Row type TR
        /// </summary>
        public static IEntityTables<TR> Tables<TR>(this (IndexedDB, TR) query)
            where TR : IEntityRow
        {
            return query.Item1.FindTables<TR>();
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

        // Select -> All
        /// <summary>
        /// This is shortcut for `indexedDB.Select<TR>().From(indexedDB.Select<TR>().Tables());
        /// Only can be used to select - because indexing requires different Row type as well
        /// </summary>
        public static (IndexedDB, TR, IEntityTables<TR>) All<TR>(this (IndexedDB, TR) query)
            where TR : IEntityRow
        {
            return (query.Item1, query.Item2, query.Tables());
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTable<TTR>, IndexQuery<TIR, TIK, TIC>, TIR)
                Where<TR, TTR, TIR, TIK, TIC>(this (IndexedDB, TR, IEntityTable<TTR>) query,
                    IIndexQueryable<TIR, TIK, TIC> index, TIK key)
            where TR : IEntityRow
            where TTR : TR, TIR
            where TIR : IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key), default);
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTables<TTR>, IndexQuery<TIR, TIK, TIC>, TIR)
                Where<TR, TTR, TIR, TIK, TIC>(this (IndexedDB, TR, IEntityTables<TTR>) query,
                    IIndexQueryable<TIR, TIK, TIC> index, TIK key)
            where TR : IEntityRow
            where TTR : TR, TIR
            where TIR : IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            return (query.Item1, query.Item2, query.Item3, index.Query(key), default);
        }

        // Select -> From Table -> Where
        // Table Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTable<TTR>, MemoBase<TMR, TMC>, TMR)
                Where<TR, TTR, TMR, TMC>(this (IndexedDB, TR, IEntityTable<TTR>) query, MemoBase<TMR, TMC> memo)
            where TR : IEntityRow
            where TTR : TR, TMR
            where TMR : IEntityRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return (query.Item1, query.Item2, query.Item3, memo, default);
        }

        // Select -> From Tables -> Where
        // Tables Row must implement both Selector Row and Index Row
        public static (IndexedDB, TR, IEntityTables<TTR>, MemoBase<TMR, TMC>, TMR)
                Where<TR, TTR, TMR, TMC>(this (IndexedDB, TR, IEntityTables<TTR>) query, MemoBase<TMR, TMC> memo)
            where TR : IEntityRow
            where TTR : TR, TMR
            where TMR : IEntityRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            return (query.Item1, query.Item2, query.Item3, memo, default);
        }

        // Select -> From Table -> Where -> Indices
        internal static IndexedIndices Indices<TR, TI, TIR>(
                this (IndexedDB, TR, IEntityTable, TI, TIR) query)
            where TR : IEntityRow
            where TI : IIndexQuery<TIR>
            where TIR : IEntityRow
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
    }

    public static class RowQueryNativeExtensions
    {
        // Select -> From Table -> Entity
        public static ref T1 Entity<TR, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTable<TR>) query, uint entityID)
            where TR : IEntityRow<T1>
            where T1 : unmanaged, IEntityComponent
        {
            return ref query.Item1.entitiesDB.QueryEntity<T1>(
                new EGID(entityID, query.Item3.ExclusiveGroup));
        }
    }

    public static class RowQueryManagedExtensions
    {
        // Select -> From Table -> Entity
        public static ref T1 Entity<TR, T1>(
                this (IndexedDB, IEntityRow<T1>, IEntityTable<TR>) query, uint entityID)
            where TR : IEntityRow<T1>
            where T1 : struct, IEntityViewComponent
        {
            return ref query.Item1.entitiesDB.QueryEntity<T1>(
                new EGID(entityID, query.Item3.ExclusiveGroup));
        }
    }
}