using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // TODO: if apply new filter system, we can remove handling for 'MovedTo' and 'Remove'
    // but we still need 'Add' to make sure new entities are included in index
    internal class TableIndexingEngine<TR, TC>
            : IReactRowAddAndRemove<TR, TC>, IReactRowSwap<TR, TC>
        where TR : class, IReactiveRow<TC>
        where TC : unmanaged, IIndexableComponent
    {
        public IndexedDB indexedDB { get; }

        public TableIndexingEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public void Add(ref TC component, IEntityTable<TR> table, uint entityID)
        {
            component.UpdateIndex<TC>(indexedDB);
        }

        public void MovedTo(ref TC component, IEntityTable<TR> previousTable, IEntityTable<TR> table, uint entityID)
        {
            component.UpdateIndex<TC>(indexedDB);
        }

        public void Remove(ref TC component, IEntityTable<TR> table, uint entityID)
        {
            component.RemoveFromIndex<TC>(indexedDB);
        }
    }
}
