using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        internal void AddMemo<T>(Memo<T> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where T : IMemorableRow
        {
            var mapper = entitiesDB.QueryMappedEntities<EGIDComponent>(groupID);

            ref var groupData = ref CreateOrGetMemoGroup<EGIDComponent>(memo._memoID, groupID);

            groupData.filter.Add(entityID, mapper);
        }

        internal void RemoveMemo<T>(Memo<T> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where T : IMemorableRow
        {
            ref var groupData = ref CreateOrGetMemoGroup<EGIDComponent>(memo._memoID, groupID);

            groupData.filter.TryRemove(entityID);
        }

        internal void ClearMemo<T>(Memo<T> memo)
            where T : IMemorableRow
        {
            if (memos.ContainsKey(memo._memoID))
                memos[memo._memoID].Clear();
        }

        internal void ClearMemos()
        {
            var values = memos.GetValues(out var count);

            for (int i = 0; i < count; ++i)
                values[i].Clear();
        }

        internal ref IndexedGroupData CreateOrGetMemoGroup<T>(int memoID, in ExclusiveGroupStruct groupID)
            where T : struct, IEntityComponent
        {
            ref var setData = ref memos.GetOrCreate(memoID, () => new IndexedKeyData
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexedGroupData>()
            });

            if (!setData.groups.ContainsKey(groupID))
            {
                setData.groups[groupID] = new IndexedGroupData
                {
                    group = groupID,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<T>(GenerateFilterId(), groupID)
                };
            }

            return ref setData.groups.GetValueByRef(groupID);
        }
    }
}
