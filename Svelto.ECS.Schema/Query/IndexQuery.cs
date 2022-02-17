using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly ref struct IndexQuery<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        internal readonly int indexerId;
        internal readonly T key;

        internal IndexQuery(int indexerId, in T key)
        {
            this.indexerId = indexerId;
            this.key = key;
        }

        internal FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(SchemaContext context)
        {
            if (!context.indexers.ContainsKey(indexerId))
                return null;

            var indexerData = (SchemaContext.IndexerData<T>)context.indexers[indexerId];
            indexerData.TryGetValue(key, out var result);
            return result;
        }
    }
}