using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static FasterList<T> ToFasterList<T>(this IEnumerable<T> enumerable)
        {
            return new FasterList<T>(enumerable.ToArray());
        }

        // public static Lazy<FasterList<T>> ToLazyFasterList<T>(this IEnumerable<T> enumerable)
        // {
        //     return new Lazy<FasterList<T>>(() => new FasterList<T>(enumerable.ToArray()));
        // }

        private static FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(this SchemaContext context, IndexQuery query)
        {
            int indexerId = query.indexerId;
            var keyToGroups = context.indexers[indexerId].keyToGroups;

            if (keyToGroups == null)
                return null;

            keyToGroups.TryGetValue(query.key, out var result);
            return result;
        }

        // we can fetch any component as long as it's part of TDescriptor
        // it doesn't have to be KeyComponent because we can assume it all share same index
        public static IEnumerable<((NB<T1>, FilteredIndices), ExclusiveGroupStruct)> QueryEntities<T1>(this SchemaContext context, IndexQuery query)
            where T1 : unmanaged, IEntityComponent
        {
            var groupDataList = context.GetGroupIndexDataList(query);

            if (groupDataList == null)
                yield break;

            var values = groupDataList.unsafeValues;

            for (int i = 0; i < groupDataList.count; ++i)
            {
                var groupData = values[i];

                if (!groupData.group.IsEnabled())
                    continue;

                var (c1, _) = context.entitiesDB.QueryEntities<T1>(groupData.group);
                var indices = groupData.filter.filteredIndices;

                yield return ((c1, indices), groupData.group);
            }
        }

        public static IEnumerable<((NB<T1>, NB<T2>, FilteredIndices), ExclusiveGroupStruct)> QueryEntities<T1, T2>(this SchemaContext context, IndexQuery query)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
        {
            var groupDataList = context.GetGroupIndexDataList(query);

            if (groupDataList == null)
                yield break;

            var values = groupDataList.unsafeValues;

            for (int i = 0; i < groupDataList.count; ++i)
            {
                var groupData = values[i];

                if (!groupData.group.IsEnabled())
                    continue;

                var (c1, c2, _) = context.entitiesDB.QueryEntities<T1, T2>(groupData.group);
                var indices = groupData.filter.filteredIndices;

                yield return ((c1, c2, indices), groupData.group);
            }
        }

        public static IEnumerable<((NB<T1>, NB<T2>, NB<T3>, FilteredIndices), ExclusiveGroupStruct)> QueryEntities<T1, T2, T3>(this SchemaContext context, IndexQuery query)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            var groupDataList = context.GetGroupIndexDataList(query);

            if (groupDataList == null)
                yield break;

            var values = groupDataList.unsafeValues;

            for (int i = 0; i < groupDataList.count; ++i)
            {
                var groupData = values[i];

                if (!groupData.group.IsEnabled())
                    continue;

                var (c1, c2, c3, _) = context.entitiesDB.QueryEntities<T1, T2, T3>(groupData.group);
                var indices = groupData.filter.filteredIndices;

                yield return ((c1, c2, c3, indices), groupData.group);
            }
        }

        public static IEnumerable<((NB<T1>, NB<T2>, NB<T3>, NB<T4>, FilteredIndices), ExclusiveGroupStruct)> QueryEntities<T1, T2, T3, T4>(this SchemaContext context, IndexQuery query)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
            where T4 : unmanaged, IEntityComponent
        {
            var groupDataList = context.GetGroupIndexDataList(query);

            if (groupDataList == null)
                yield break;

            var values = groupDataList.unsafeValues;

            for (int i = 0; i < groupDataList.count; ++i)
            {
                var groupData = values[i];

                if (!groupData.group.IsEnabled())
                    continue;

                var (c1, c2, c3, c4, _) = context.entitiesDB.QueryEntities<T1, T2, T3, T4>(groupData.group);
                var indices = groupData.filter.filteredIndices;

                yield return ((c1, c2, c3, c4, indices), groupData.group);
            }
        }
    }
}