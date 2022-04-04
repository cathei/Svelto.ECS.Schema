using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class TableIndexingEngine<TR, TC, TK> :
            IReactRowAdd<TR, ResultSet<TC>>,
            IReactRowRemove<TR, ResultSet<TC>>
        where TR : class, IQueryableRow<ResultSet<TC>>
        where TC : unmanaged, IKeyComponent
        where TK : unmanaged, IEquatable<TK>
    {
        public TableIndexingEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        private static readonly RefWrapperType ComponentType = TypeRefWrapper<TC>.wrapper;

        public IndexedDB indexedDB { get; }

        public void Add(in ResultSet<TC> resultSet, RangedIndices indices, ExclusiveGroupStruct group)
        {
            foreach (var i in indices)
            {
                indexedDB.UpdateIndexableComponent(ComponentType, new EGID(resultSet.entityIDs[i], group),
                    IndexableComponentHelper<TC>.KeyGetter<TK>.Getter(ref resultSet.component[i]));
            }
        }

        public void Remove(in ResultSet<TC> resultSet, RangedIndices indices, ExclusiveGroupStruct group)
        {
            foreach (var i in indices)
            {
                indexedDB.RemoveIndexableComponent<TK>(ComponentType, new EGID(resultSet.entityIDs[i], group));
            }
        }
    }
}
