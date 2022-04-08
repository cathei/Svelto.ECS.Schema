using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class ForeignKeyEngine<TComponent> :
            IReactRowAdd<IForeignKeyRow<TComponent>, TComponent>,
            IReactRowRemove<IForeignKeyRow<TComponent>, TComponent>,
            IReactRowSwap<IReferenceableRow<TComponent>, Referenceable<TComponent>>,
            IReactRowRemove<IReferenceableRow<TComponent>, Referenceable<TComponent>>
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

            foreach (var i in indices)
            {
                indexedDB.UpdateForeignKeyComponent<TComponent>(
                    new(entityIDs[i], group), component[i].reference);
            }
        }

        public void Remove(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (_, entityIDs, _) = collection;

            foreach (var i in indices)
            {
                indexedDB.RemoveForeignKeyComponent<TComponent>(new(entityIDs[i], group));
            }
        }

        public void MovedTo(in EntityCollection<Referenceable<TComponent>> collection, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var (_, entityIDs, _) = collection;

            foreach (var i in indices)
            {
                var other = indexedDB.GetEntityReference(entityIDs[i], toGroup);
                indexedDB.UpdateReferencedComponent<TComponent>(other);
            }
        }

        public void Remove(in EntityCollection<Referenceable<TComponent>> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (_, entityIDs, _) = collection;

            foreach (var i in indices)
            {
                var other = indexedDB.GetEntityReference(entityIDs[i], group);
                indexedDB.RemoveReferencedComponent<TComponent>(other);
            }
        }
    }
}
