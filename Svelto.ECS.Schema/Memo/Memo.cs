using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    internal static class GlobalMemoCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public sealed partial class Memo<T> : ISchemaDefinitionMemo, IEntityIndexQuery
        where T : struct, IEntityComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _memoID = GlobalMemoCount.Generate();

        int ISchemaDefinitionMemo.MemoID => _memoID;

        public void Add(IndexedDB indexedDB, EGID egid)
        {
            indexedDB.AddMemo(this, egid.entityID, egid.groupID);
        }

        public void Remove(IndexedDB indexedDB, EGID egid)
        {
            indexedDB.RemoveMemo(this, egid.entityID, egid.groupID);
        }

        public void Clear(IndexedDB indexedDB)
        {
            indexedDB.ClearMemo(this);
        }

        // we have type constraints because it requires INeedEGID
        // it won't be necessary when Svelto update it's filter utility functions
        internal void Set<TQuery, TC>(IndexedDB indexedDB, TQuery query)
            where TQuery : IEntityIndexQuery<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            Clear(indexedDB);
            Union<TQuery, TC>(indexedDB, query);
        }

        internal void Union<TQuery, TC>(IndexedDB indexedDB, TQuery query)
            where TQuery : IEntityIndexQuery<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            var queryData = query.GetIndexedKeyData(indexedDB).groups;

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

                var mapper = indexedDB.entitiesDB.QueryMappedEntities<T>(queryGroupData.group);

                var components = query.GetComponents(indexedDB, queryGroupData.group);
                ref var originalGroupData = ref indexedDB.CreateOrGetMemoGroup<T>(_memoID, queryGroupData.group);

                foreach (var i in new IndexedIndices(queryGroupData.filter.filteredIndices))
                    originalGroupData.filter.Add(components[i].ID.entityID, mapper);
            }
        }

        internal void Intersect<TQuery, TC>(IndexedDB indexedDB, TQuery query)
            where TQuery : IEntityIndexQuery<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            var originalData = GetIndexedKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (originalData == null)
                return;

            var queryData = query.GetIndexedKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (queryData == null)
            {
                Clear(indexedDB);
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
                if (!queryData.TryGetValue(originalDataValues[groupIndex].group, out var queryGroupData) ||
                    queryGroupData.filter.filteredIndices.Count() == 0)
                {
                    originalGroupData.filter.Clear();
                    continue;
                }

                var components = query.GetComponents(indexedDB, queryGroupData.group);

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

        private IndexedKeyData GetIndexedKeyData(IndexedDB indexedDB)
        {
            indexedDB.memos.TryGetValue(_memoID, out var result);
            return result;
        }

        IndexedKeyData IEntityIndexQuery.GetIndexedKeyData(IndexedDB indexedDB)
            => GetIndexedKeyData(indexedDB);
    }
}