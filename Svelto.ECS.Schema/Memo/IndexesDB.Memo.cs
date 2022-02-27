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
            where T : struct, IEntityComponent
        {
            var mapper = entitiesDB.QueryMappedEntities<T>(groupID);

            ref var groupData = ref CreateOrGetMemoGroup<T>(memo._memoID, groupID);

            groupData.filter.Add(entityID, mapper);
        }

        internal void RemoveMemo<T>(Memo<T> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where T : struct, IEntityComponent
        {
            ref var groupData = ref CreateOrGetMemoGroup<T>(memo._memoID, groupID);

            groupData.filter.TryRemove(entityID);
        }

        internal void ClearMemo<T>(T memo) where T : ISchemaDefinitionMemo
        {
            if (memos.ContainsKey(memo.MemoID))
                memos[memo.MemoID].Clear();
        }

        internal void ClearMemos()
        {
            var values = memos.GetValues(out var count);

            for (int i = 0; i < count; ++i)
                values[i].Clear();
        }

        internal ref IndexerGroupData CreateOrGetMemoGroup<T>(int memoID, in ExclusiveGroupStruct groupID)
            where T : struct, IEntityComponent
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
