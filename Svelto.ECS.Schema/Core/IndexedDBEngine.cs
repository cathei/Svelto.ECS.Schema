using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    internal class IndexedDBEngine :
        UnsortedEnginesGroup<IStepEngine>,
        IQueryingEntitiesEngine
    {
        private readonly IndexedDB _indexedDB;

        public EntitiesDB entitiesDB { private get; set; }

        public IndexedDBEngine(IndexedDB indexedDB, FasterList<IStepEngine> engines) : base(engines)
        {
            _indexedDB = indexedDB;
        }

        public void Ready()
        {
            // this seems like only way to inject entitiesDB...
            _indexedDB.entitiesDB = entitiesDB;
        }
    }
}
