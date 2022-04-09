using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexedDB
    {
        internal void AddMemo<TR>(MemoBase<TR> memo, in EGID egid)
            where TR : class, IEntityRow
        {
            if (FindTable<TR>(egid.groupID) == null)
                return;

            var filter = memo.GetFilter(this);
            var mapper = GetNativeEGIDMapper(egid.groupID);

            filter.Add(egid, mapper);
        }

        internal void RemoveMemo<TR>(MemoBase<TR> memo, in EGID egid)
            where TR : class, IEntityRow
        {
            var filter = memo.GetFilter(this);
            filter.Remove(egid);
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
