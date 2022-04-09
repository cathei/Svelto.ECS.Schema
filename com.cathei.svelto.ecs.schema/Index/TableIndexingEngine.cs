using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class TableIndexingEngine<TRow, TComponent> :
            IReactRowAdd<TRow, TComponent>,
            IReactRowRemove<TRow, TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        public TableIndexingEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        public void Add(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (component, entityIDs, _) = collection;

            foreach (var i in indices)
            {
                KeyComponentHelper<TComponent>.Handler.Update(indexedDB, ref component[i], new(entityIDs[i], group));
            }
        }

        public void Remove(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (identity, _) = indexedDB.entitiesDB.QueryEntities<RowIdentityComponent>(group);

            foreach (var i in indices)
            {
                KeyComponentHelper<TComponent>.Handler.Remove(indexedDB, identity[i].selfReference);
            }
        }
    }
}
