using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        internal void AddMemo<TR>(MemoBase<TR> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where TR : class, IEntityRow
        {
            if (FindTable<TR>(groupID) == null)
                return;

            var filter = memo.GetFilter(this);
            var mapper = GetNativeEGIDMapper(groupID);

            filter.Add(new EGID(entityID, groupID), mapper);
        }

        internal void RemoveMemo<TR>(MemoBase<TR> memo, uint entityID, in ExclusiveGroupStruct groupID)
            where TR : class, IEntityRow
        {
            var filter = memo.GetFilter(this);
            filter.Remove(new EGID(entityID, groupID));
        }

        internal void ClearMemo(MemoBase memo)
        {
            var filter = memo.GetFilter(this);
            filter.Clear();
        }

        internal ref EntityFilterCollection GetOrAddPersistentFilter(CombinedFilterID filterID)
        {
            return ref entitiesDB.GetFilters().GetOrCreatePersistentFilter<RowIdentityComponent>(filterID);
        }

        internal ref EntityFilterCollection GetOrAddTransientFilter(CombinedFilterID filterID)
        {
            return ref entitiesDB.GetFilters().GetOrCreateTransientFilter<RowIdentityComponent>(filterID);
        }
    }
}
