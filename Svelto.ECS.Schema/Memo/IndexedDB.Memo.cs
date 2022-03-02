using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        internal void AddMemo(MemoBase memo, uint entityID, in ExclusiveGroupStruct groupID)
        {
            var mapper = entitiesDB.QueryMappedEntities<EGIDComponent>(groupID);

            ref var groupData = ref CreateOrGetMemoGroup<EGIDComponent>(memo._memoID, groupID);

            groupData.filter.Add(entityID, mapper);
        }

        internal void RemoveMemo(MemoBase memo, uint entityID, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref CreateOrGetMemoGroup<EGIDComponent>(memo._memoID, groupID);

            groupData.filter.TryRemove(entityID);
        }

        internal void ClearMemo(MemoBase memo)
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

        internal ref IndexedGroupData<TR> CreateOrGetMemoGroup<TR, TC>(int memoID, IEntityTable<TR> table)
            where TR : IEntityRow<TC>
            where TC : struct, IEntityComponent
        {
            ref var setData = ref memos.GetOrCreate(memoID, () => new IndexedKeyData<TR>
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TR>>()
            });

            if (!setData.groups.ContainsKey(table.ExclusiveGroup))
            {
                setData.groups[table.ExclusiveGroup] = new IndexedGroupData<TR>
                {
                    table = table,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<TC>(GenerateFilterId(), table.ExclusiveGroup)
                };
            }

            return ref setData.groups.GetValueByRef(table.ExclusiveGroup);
        }
    }
}
