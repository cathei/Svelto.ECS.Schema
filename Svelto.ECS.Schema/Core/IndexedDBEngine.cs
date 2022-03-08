using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    internal class IndexedDBEngine : IQueryingEntitiesEngine,
        IReactOnAddAndRemove<RowIdentityComponent>, IReactOnSwap<RowIdentityComponent>, IReactOnSubmission
    {
        private readonly IndexedDB _indexedDB;

        private readonly HashSet<ExclusiveGroupStruct> _groupsToRebuild = new HashSet<ExclusiveGroupStruct>();

        public EntitiesDB entitiesDB { private get; set; }

        public IndexedDBEngine(IndexedDB indexedDB)
        {
            _indexedDB = indexedDB;
        }

        public void Ready()
        {
            // this seems like only way to inject entitiesDB...
            _indexedDB.entitiesDB = entitiesDB;
        }

        public void Add(ref RowIdentityComponent entityComponent, EGID egid) { }

        public void Remove(ref RowIdentityComponent entityComponent, EGID egid)
        {
            _groupsToRebuild.Add(egid.groupID);
        }

        public void MovedTo(ref RowIdentityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            _groupsToRebuild.Add(previousGroup);
        }

        public void EntitiesSubmitted()
        {
            _indexedDB.RebuildFilters(_groupsToRebuild);

            // Even if we call it here, we still need to manually clean memo because
            // EntitiesSubmitted only called when there is entities to submit
            // We don't have to do this anyway if we move to new filter system
            // _indexedDB.ClearMemos();
        }
    }
}
