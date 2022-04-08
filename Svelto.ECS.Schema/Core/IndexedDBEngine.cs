using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    internal class IndexedDBEngine :
        UnsortedEnginesGroup<IStepEngine>,
        IQueryingEntitiesEngine,
        IReactOnAddEx<RowIdentityComponent>
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

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<RowIdentityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var (component, entityIDs, _) = collection;

            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; ++i)
            {
                component[i].selfReference = entitiesDB.GetEntityReference(new EGID(entityIDs[i], groupID));
            }
        }
    }
}
