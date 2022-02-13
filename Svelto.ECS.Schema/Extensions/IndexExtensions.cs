using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    public static partial class IndexExtensions
    {
        private static readonly FilteredIndices EmptyFilteredIndices = new FilteredIndices();

        private static FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(this SchemaContext context, IndexQuery query)
        {
            int indexerId = query.indexerId;
            var keyToGroups = context.indexers[indexerId].keyToGroups;

            if (keyToGroups == null)
                return null;

            keyToGroups.TryGetValue(query.key, out var result);
            return result;
        }
    }
}