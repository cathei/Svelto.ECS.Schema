using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexesDB
    {
        internal void AddMemo<T>(Memo<T> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where T : unmanaged, IEntityComponent
        {
            var mapper = entitiesDB.QueryMappedEntities<T>(groupID);

            ref var groupData = ref CreateOrGetMemoGroup<T>(memo._memoID, groupID);

            groupData.filter.Add(entityID, mapper);
        }

        internal void RemoveMemo<T>(Memo<T> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where T : unmanaged, IEntityComponent
        {
            ref var groupData = ref CreateOrGetMemoGroup<T>(memo._memoID, groupID);

            groupData.filter.TryRemove(entityID);
        }

        internal void ClearMemo<T>(T memo) where T : IEntitySchemaMemo
        {
            memos[memo.MemoID].Clear();
        }

        internal void ClearMemos()
        {
            for (int i = 0; i < memos.count; ++i)
                memos.unsafeValues[i].Clear();
        }

        internal ref IndexerGroupData CreateOrGetMemoGroup<T>(int memoID, in ExclusiveGroupStruct groupID)
            where T : unmanaged, IEntityComponent
        {
            ref var setData = ref memos.GetOrCreate(memoID, () => new IndexerSetData
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>()
            });

            if (!setData.groups.ContainsKey(groupID))
            {
                setData.groups[groupID] = new IndexerGroupData
                {
                    group = groupID,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<T>(GenerateFilterId(), groupID)
                };
            }

            return ref setData.groups.GetValueByRef(groupID);
        }
    }
}
