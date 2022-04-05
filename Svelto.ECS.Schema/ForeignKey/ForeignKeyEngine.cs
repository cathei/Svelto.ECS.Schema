using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // cannot use IReactRow because any row should be able to be referenced
    internal class ForeignKeyEngine<TC, TR> :
            IReactOnAddEx<RowIdentityComponent>,
            IReactOnRemoveEx<RowIdentityComponent>,
            IReactOnSwapEx<RowIdentityComponent>
        where TC : unmanaged, IForeignKeyComponent
        where TR : class, IEntityRow
    {
        public ForeignKeyEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<RowIdentityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable<TR>(groupID);

            if (table == null)
                return;


        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<RowIdentityComponent> collection, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var table = indexedDB.FindTable<TR>(toGroup);

            if (table == null)
                return;


        }


        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<RowIdentityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable<TR>(groupID);

            if (table == null)
                return;

        }
    }
}
