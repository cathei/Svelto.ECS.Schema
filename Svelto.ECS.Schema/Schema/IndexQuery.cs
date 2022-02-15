using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly partial struct IndexQuery<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        internal readonly int indexerId;
        internal readonly T key;

        private static readonly FilteredIndices EmptyFilteredIndices = new FilteredIndices();

        internal IndexQuery(int indexerId, T key)
        {
            this.indexerId = indexerId;
            this.key = key;
        }

        private FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(SchemaContext context)
        {
            if (context.indexers[indexerId] == null)
                return null;

            var indexerData = (SchemaContext.IndexerData<T>)context.indexers[indexerId];
            indexerData.TryGetValue(key, out var result);
            return result;
        }
    }
}