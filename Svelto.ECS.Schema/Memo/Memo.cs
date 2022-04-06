using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalMemoCount
    {
        internal static FilterContextID MemoContextID = EntitiesDB.SveltoFilters.GetNewContextID();
        private static int Count = 0;

        public static CombinedFilterID Generate() => new(Interlocked.Increment(ref Count), MemoContextID);
    }

    public abstract class MemoBase : IEntityMemo
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly CombinedFilterID _filterID = GlobalMemoCount.Generate();

        CombinedFilterID IEntityMemo.FilterID => _filterID;

        internal MemoBase() { }

        internal ref EntityFilterCollection GetFilter(IndexedDB indexedDB)
        {
            return ref indexedDB.GetOrAddTransientFilter(_filterID);
        }
    }

    public abstract class MemoBase<TRow> : MemoBase, IIndexQuery<TRow>
        where TRow : class, IEntityRow
    {
        internal MemoBase() { }

        internal void Set<TIndex>(IndexedDB indexedDB, TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            indexedDB.ClearMemo(this);
            Union(indexedDB, indexQuery);
        }

        internal void Union<TIndex>(IndexedDB indexedDB, TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            ref var originalFilter = ref GetFilter(indexedDB);

            foreach (var query in indexedDB.From<TRow>().Where(indexQuery))
            {
                var groupFilter = originalFilter.GetGroupFilter(query.group);

                foreach (var i in query.indices)
                    groupFilter.Add(query.egid[i].entityID, i);
            }
        }

        internal void Intersect<TIndex>(IndexedDB indexedDB, TIndex other)
            where TIndex : IIndexQuery<TRow>
        {
            ref var originalFilter = ref GetFilter(indexedDB);

            var otherQueries = indexedDB.From<TRow>().Where(other);

            otherQueries.Build();

            for (int groupIndex = 0; groupIndex < originalFilter.groupCount; ++groupIndex)
            {
                var groupFilter = originalFilter.GetGroup(groupIndex);

                // if group is empty there is nothing to remove
                if (groupFilter.count == 0)
                    continue;

                // if target is empty there is no intersection
                if (!otherQueries.config.temporaryGroups.ContainsKey(groupFilter.group))
                    groupFilter.Clear();
            }

            foreach (var otherQuery in otherQueries)
            {
                var groupFilter = originalFilter.GetGroupFilter(otherQuery.group);

                // nothing to remove
                if (groupFilter.count == 0)
                    continue;

                var indices = groupFilter.indices;

                // reverse iteration so we can remove safely
                for (int i = (int)(indices.count - 1); i >= 0; --i)
                {
                    var entityID = otherQuery.egid[indices[i]].entityID;

                    if (!otherQuery.indices.IndexExists(indices[i]))
                        groupFilter.Remove(entityID);
                }
            }
        }

        void IIndexQuery.Apply(ResultSetQueryConfig config)
        {
            config.filters.Add(GetFilter(config.indexedDB));
        }
    }
}

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Memo<TRow> : MemoBase<TRow>
        where TRow : class, IEntityRow
    { }

    public sealed class Memo : MemoBase<IEntityRow>
    { }
}