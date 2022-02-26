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
        where T : unmanaged, IEntityComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _memoID = GlobalMemoCount.Generate();

        int ISchemaDefinitionMemo.MemoID => _memoID;

        public void Add(IndexesDB indexesDB, EGID egid)
        {
            indexesDB.AddMemo(this, egid.entityID, egid.groupID);
        }

        public void Remove(IndexesDB indexesDB, EGID egid)
        {
            indexesDB.RemoveMemo(this, egid.entityID, egid.groupID);
        }

        public void Clear(IndexesDB indexesDB)
        {
            indexesDB.ClearMemo(this);
        }

        internal void Set<TQuery, TK, TC>(IndexesDB indexesDB, TQuery query)
            where TQuery : IEntityIndexQuery<TK, TC>
            where TK : unmanaged
            where TC : unmanaged, IIndexedComponent<TK>
        {
            Clear(indexesDB);
            Union<TQuery, TK, TC>(indexesDB, query);
        }

        internal void Union<TQuery, TK, TC>(IndexesDB indexesDB, TQuery query)
            where TQuery : IEntityIndexQuery<TK, TC>
            where TK : unmanaged
            where TC : unmanaged, IIndexedComponent<TK>
        {
            var queryData = query.GetGroupIndexDataList(indexesDB).groups;

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

                var mapper = indexesDB.entitiesDB.QueryMappedEntities<T>(queryGroupData.group);

                var components = query.GetComponents(indexesDB, queryGroupData.group);
                ref var originalGroupData = ref indexesDB.CreateOrGetMemoGroup<T>(_memoID, queryGroupData.group);

                foreach (var i in new IndexedIndices(queryGroupData.filter.filteredIndices))
                    originalGroupData.filter.Add(components[i].ID.entityID, mapper);
            }
        }

        internal void Intersect<TQuery, TK, TC>(IndexesDB indexesDB, TQuery query)
            where TQuery : IEntityIndexQuery<TK, TC>
            where TK : unmanaged
            where TC : unmanaged, IIndexedComponent<TK>
        {
            var originalData = GetGroupIndexDataList(indexesDB).groups;

            // if empty nothing to intersect
            if (originalData == null)
                return;

            var queryData = query.GetGroupIndexDataList(indexesDB).groups;

            // if empty nothing to intersect
            if (queryData == null)
            {
                Clear(indexesDB);
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

                var components = query.GetComponents(indexesDB, queryGroupData.group);

                // ugh I have to check what to delete
                // since I cannot change filter while iteration
                FasterList<uint> entityIDsToDelete = new FasterList<uint>();

                foreach (uint i in new IndexedIndices(originalGroupData.filter.filteredIndices))
                {
                    if (!queryGroupData.filter.Exists(components[i].ID.entityID))
                        entityIDsToDelete.Add(i);
                }

                for (int i = 0; i < entityIDsToDelete.count; ++i)
                {
                    originalGroupData.filter.TryRemove(entityIDsToDelete[i]);
                }
            }
        }

        private IndexesDB.IndexerSetData GetGroupIndexDataList(IndexesDB indexesDB)
        {
            indexesDB.memos.TryGetValue(_memoID, out var result);
            return result;
        }

        IndexesDB.IndexerSetData IEntityIndexQuery.GetGroupIndexDataList(IndexesDB indexesDB)
            => GetGroupIndexDataList(indexesDB);
    }
}