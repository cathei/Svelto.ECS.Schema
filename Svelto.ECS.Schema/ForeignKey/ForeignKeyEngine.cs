using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class ForeignKeyEngine<TComponent> :
            IReactRowAdd<IForeignKeyRow<TComponent>, TComponent>,
            IReactRowRemove<IForeignKeyRow<TComponent>, TComponent>,
            IReactRowSwap<IReferenceableRow<TComponent>, Refereneable<TComponent>>,
            IReactRowRemove<IReferenceableRow<TComponent>, Refereneable<TComponent>>
        where TComponent : unmanaged, IForeignKeyComponent
    {
        public ForeignKeyEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        // this can help use reuse index backend without affecting other indexers
        private static readonly RefWrapperType ForeignKeyType = TypeRefWrapper<ForeignKey<TComponent>>.wrapper;

        public void Add(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (component, entityIDs, _) = collection;

            foreach (var i in indices)
            {
                indexedDB.UpdateForeignKeyComponent<TComponent>(
                    ForeignKeyType, new(entityIDs[i], group), component[i].reference);
            }
        }

        public void Remove(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (component, entityIDs, _) = collection;

            foreach (var i in indices)
            {
                indexedDB.RemoveForeignKeyComponent(ForeignKeyType, new(entityIDs[i], group));
            }
        }

        public void MovedTo(in EntityCollection<Refereneable<TComponent>> collection, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var (_, entityIDs, _) = collection;

            foreach (var i in indices)
            {

            }
        }

        public void Remove(in EntityCollection<Refereneable<TComponent>> collection, RangedIndices indices, ExclusiveGroupStruct group)
        {
            var (_, entityIDs, _) = collection;

            foreach (var i in indices)
            {
            }
        }
    }
}
