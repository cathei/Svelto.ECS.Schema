using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class ForeignKeyEngine<TComponent> :
            IReactRowAdd<IForeignKeyRow<TComponent>, TComponent>,
            IReactRowRemove<IForeignKeyRow<TComponent>, TComponent>
        where TComponent : unmanaged, IForeignKeyComponent
    {
        public ForeignKeyEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        public void Add(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (component, entityIDs, _) = collection;
            var (identity, _) = indexedDB.entitiesDB.QueryEntities<RowIdentityComponent>(group);

            foreach (var i in indices)
            {
                indexedDB.UpdateForeignKeyComponent<TComponent>(
                    new(entityIDs[i], group), identity[i].selfReference, component[i].reference);
            }
        }

        public void Remove(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (identity, _) = indexedDB.entitiesDB.QueryEntities<RowIdentityComponent>(group);

            foreach (var i in indices)
            {
                indexedDB.RemoveForeignKeyComponent<TComponent>(identity[i].selfReference);
            }
        }
    }

    internal class ForeignKeyReferenceableEngine<TComponent> :
            IReactRowSwap<IReferenceableRow<TComponent>, Referenceable<TComponent>>,
            IReactRowRemove<IReferenceableRow<TComponent>, Referenceable<TComponent>>
        where TComponent : unmanaged, IForeignKeyComponent
    {
        public ForeignKeyReferenceableEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        public void MovedTo(in EntityCollection<Referenceable<TComponent>> collection, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var (identity, _) = indexedDB.entitiesDB.QueryEntities<RowIdentityComponent>(toGroup);

            foreach (var i in indices)
            {
                indexedDB.UpdateReferencedComponent<TComponent>(identity[i].selfReference);
            }
        }

        public void Remove(in EntityCollection<Referenceable<TComponent>> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (identity, _) = indexedDB.entitiesDB.QueryEntities<RowIdentityComponent>(group);

            foreach (var i in indices)
            {
                indexedDB.RemoveReferencedComponent<TComponent>(identity[i].selfReference);
            }
        }
    }
}
