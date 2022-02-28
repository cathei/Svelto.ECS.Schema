using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    internal class IndexedDBEngine : IQueryingEntitiesEngine //, IReactOnSubmission
    {
        private readonly IndexedDB _indexedDB;

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

        public void EntitiesSubmitted()
        {
            // Even if we call it here, we still need to manually clean memo because
            // EntitiesSubmitted only called when there is entities to submit
            // We don't have to do this anyway if we move to new filter system

            // _indexedDB.ClearMemos();
        }
    }
}
