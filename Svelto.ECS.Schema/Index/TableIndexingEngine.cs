using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class TableIndexingEngine<TRow, TComponent> :
            IReactRowAdd<TRow, ResultSet<TComponent>>,
            IReactRowRemove<TRow, ResultSet<TComponent>>
        where TRow : class, IQueryableRow<ResultSet<TComponent>>
        where TComponent : unmanaged, IKeyComponent
    {
        public TableIndexingEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        public void Add(in ResultSet<TComponent> resultSet, RangedIndices indices, ExclusiveGroupStruct group)
        {
            foreach (var i in indices)
            {
                KeyComponentHelper<TComponent>.Handler.Update(indexedDB, ref resultSet.component[i],
                    new EGID(resultSet.entityIDs[i], group));
            }
        }

        public void Remove(in ResultSet<TComponent> resultSet, RangedIndices indices, ExclusiveGroupStruct group)
        {
            foreach (var i in indices)
            {
                KeyComponentHelper<TComponent>.Handler.Remove(indexedDB, new EGID(resultSet.entityIDs[i], group));
            }
        }
    }
}
