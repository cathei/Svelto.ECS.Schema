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

    public abstract class MemoBase<TRow> : IMemoDefinition, IEntityMemo<TRow>, IWhereQuery<TRow>
        where TRow : class, IEntityRow
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly CombinedFilterID _filterID = GlobalMemoCount.Generate();

        CombinedFilterID IMemoDefinition.FilterID => _filterID;

        internal MemoBase() { }

        void IEntityMemo<TRow>.Set<TWhere>(IndexedDB indexedDB, TWhere indexQuery)
        {
            indexedDB.ClearMemo(this);

            ((IEntityMemo<TRow>)this).Union(indexedDB, indexQuery);
        }

        void IEntityMemo<TRow>.Union<TWhere>(IndexedDB indexedDB, TWhere indexQuery)
        {
            ref var originalFilter = ref GetFilter(indexedDB);

            foreach (var query in indexedDB.FromAll<TRow>().Where(indexQuery))
            {
                var groupFilter = originalFilter.GetGroupFilter(query.group);

                foreach (var i in query.indices)
                    groupFilter.Add(query.egid[i].entityID, i);
            }
        }

        void IEntityMemo<TRow>.Intersect<TWhere>(IndexedDB indexedDB, TWhere other)
        {
            ref var originalFilter = ref GetFilter(indexedDB);

            var otherQueries = indexedDB.FromAll<TRow>().Where(other);

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

        internal ref EntityFilterCollection GetFilter(IndexedDB indexedDB)
        {
            return ref indexedDB.GetOrAddTransientFilter(_filterID);
        }

        ref EntityFilterCollection IEntityMemo<TRow>.GetFilter(IndexedDB indexedDB) => ref GetFilter(indexedDB);

        void IWhereQuery.Apply(ResultSetQueryConfig config)
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