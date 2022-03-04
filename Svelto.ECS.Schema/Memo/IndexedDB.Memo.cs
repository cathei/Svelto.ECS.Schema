using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        internal void AddMemo<TR, TC>(MemoBase<TR, TC> memo, uint entityID, IEntityTable<TR> table)
            where TR : class, IEntityRow<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            var mapper = entitiesDB.QueryMappedEntities<TC>(table.ExclusiveGroup);

            ref var groupData = ref CreateOrGetMemoGroup(memo._memoID, table);

            groupData.filter.Add(entityID, mapper);
        }

        internal void RemoveMemo<TR, TC>(MemoBase<TR, TC> memo, uint entityID, IEntityTable<TR> table)
            where TR : class, IEntityRow<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            ref var groupData = ref CreateOrGetMemoGroup(memo._memoID, table);

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

        internal ref IndexedGroupData<TR> CreateOrGetMemoGroup<TR>(int memoID, IEntityTable<TR> table)
            where TR : IEntityRow
        {
            var memoData = (MemoData<TR>)memos.GetOrCreate(memoID, () => new MemoData<TR>());

            if (!memoData.keyData.groups.ContainsKey(table.ExclusiveGroup))
            {
                memoData.keyData.groups[table.ExclusiveGroup] = new IndexedGroupData<TR>
                {
                    table = table,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<RowMetaComponent>(
                        GenerateFilterId(), table.ExclusiveGroup)
                };
            }

            return ref memoData.keyData.groups.GetValueByRef(table.ExclusiveGroup);
        }
    }
}
