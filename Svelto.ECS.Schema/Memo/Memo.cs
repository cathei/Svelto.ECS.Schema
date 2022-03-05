using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalMemoCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public class MemoBase : ISchemaDefinitionMemo
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _memoID = GlobalMemoCount.Generate();

        int ISchemaDefinitionMemo.MemoID => _memoID;

        internal MemoBase() { }
    }

    public class MemoBase<TRow, TComponent> : MemoBase, IIndexQuery
        where TRow : class, IEntityRow<TComponent>
        where TComponent : unmanaged, IEntityComponent, INeedEGID
    {
        internal MemoBase() { }

        internal void Set<TQ>(IndexedDB indexedDB, TQ query)
            where TQ : IIndexQuery
        {
            indexedDB.ClearMemo(this);
            Union(indexedDB, query);
        }

        internal void Union<TQ>(IndexedDB indexedDB, TQ query)
            where TQ : IIndexQuery
        {
            var queryData = query.GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to add
            if (queryData == null)
                return;

            var queryDataValues = queryData.GetValues(out var queryDataCount);

            for (int groupIndex = 0; groupIndex < queryDataCount; ++groupIndex)
            {
                var queryGroupData = queryDataValues[groupIndex];

                // if empty nothing to add
                if (queryGroupData.filter.filteredIndices.Count() == 0)
                    continue;

                var table = queryGroupData.table as IEntityTable<TRow>;

                // type mismatch - noathing to add
                if (table == null)
                    continue;

                var mapper = indexedDB.entitiesDB.QueryMappedEntities<TComponent>(table.ExclusiveGroup);

                // TODO: change group to table!
                var (components, _) = indexedDB.Select<TRow>().From(table).Entities();

                ref var originalGroupData = ref indexedDB.CreateOrGetMemoGroup(_memoID, table);

                foreach (var i in new IndexedIndices(queryGroupData.filter.filteredIndices))
                    originalGroupData.filter.Add(components[i].ID.entityID, mapper);
            }
        }

        internal void Intersect<TQ>(IndexedDB indexedDB, TQ query)
            where TQ : IIndexQuery
        {
            var originalData = GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (originalData == null)
                return;

            var queryData = query.GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (queryData == null)
            {
                indexedDB.ClearMemo(this);
                return;
            }

            var originalDataValues = originalData.GetValues(out var originalDataCount);

            for (int groupIndex = 0; groupIndex < originalDataCount; ++groupIndex)
            {
                ref var originalGroupData = ref originalDataValues[groupIndex];

                // if group is empty there is nothing to remove
                if (originalGroupData.filter.filteredIndices.Count() == 0)
                    continue;

                // if target is empty there is no intersection
                if (!queryData.TryGetValue(originalDataValues[groupIndex].table.ExclusiveGroup, out var queryGroupData) ||
                    queryGroupData.filter.filteredIndices.Count() == 0)
                {
                    originalGroupData.filter.Clear();
                    continue;
                }

                var table = queryGroupData.table as IEntityTable<TRow>;

                // type mismatch - no intersection
                if (table == null)
                {
                    originalGroupData.filter.Clear();
                    continue;
                }

                var (components, _) = indexedDB.Select<TRow>().From(table).Entities();

                // ugh I have to check what to delete
                // since I cannot change filter while iteration
                // this will be removed when Svelto updates it's filter system
                FasterList<uint> entityIDsToDelete = new FasterList<uint>();

                foreach (uint i in new IndexedIndices(originalGroupData.filter.filteredIndices))
                {
                    if (!queryGroupData.filter.Exists(components[i].ID.entityID))
                        entityIDsToDelete.Add(components[i].ID.entityID);
                }

                for (int i = 0; i < entityIDsToDelete.count; ++i)
                {
                    originalGroupData.filter.TryRemove(entityIDsToDelete[i]);
                }
            }
        }

        private IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB)
        {
            if (indexedDB.memos.TryGetValue(_memoID, out var result))
                return result.keyData;
            return default;
        }

        IndexerKeyData IIndexQuery.GetIndexerKeyData(IndexedDB indexedDB)
            => GetIndexerKeyData(indexedDB);
    }
}

namespace Svelto.ECS.Schema.Definition
{
    // we have EGIDComponent constraints because it requires INeedEGID
    // it won't be necessary when Svelto update it's filter utility functions
    public interface IMemorableRow : IEntityRow<EGIDComponent> { }

    public sealed class Memo<TRow> : MemoBase<TRow, EGIDComponent>
        where TRow : class, IMemorableRow
    { }
}