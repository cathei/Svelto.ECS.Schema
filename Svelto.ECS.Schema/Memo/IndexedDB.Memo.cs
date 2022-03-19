using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        internal void AddMemo<TR, TC>(MemoBase<TR, TC> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where TR : class, IIndexableRow<TC>
            where TC : unmanaged, IEntityComponent
        {
            if (FindTable<TR>(groupID) == null)
                return;

            var mapper = GetEGIDMapper(groupID);

            ref var groupData = ref CreateOrGetMemoGroup(memo._memoID, groupID);

            groupData.filter.Add(entityID, mapper);
        }

        internal void RemoveMemo<TR, TC>(MemoBase<TR, TC> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where TR : class, IIndexableRow<TC>
            where TC : unmanaged, IEntityComponent
        {
            ref var groupData = ref CreateOrGetMemoGroup(memo._memoID, groupID);

            groupData.filter.TryRemove(entityID);
        }

        internal void ClearMemo(MemoBase memo)
        {
            if (memos.TryGetValue(memo._memoID, out var memoData))
                memoData.Clear();
        }

        internal void ClearMemos()
        {
            var values = memos.GetValues(out var count);
            for (int i = 0; i < count; ++i)
                values[i].Clear();
        }

        internal ref IndexerGroupData CreateOrGetMemoGroup(int memoID, in ExclusiveGroupStruct groupID)
        {
            var memoData = memos.GetOrCreate(memoID, () => new MemoData());

            if (!memoData.keyData.groups.ContainsKey(groupID))
            {
                memoData.keyData.groups[groupID] = new IndexerGroupData
                {
                    groupID = groupID,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<RowIdentityComponent>(
                        GenerateFilterId(), groupID)
                };
            }

            return ref memoData.keyData.groups.GetValueByRef(groupID);
        }
    }
}
