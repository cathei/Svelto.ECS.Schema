using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalMemoCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public abstract class MemoBase : ISchemaDefinitionMemo
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _memoID = GlobalMemoCount.Generate();

        int ISchemaDefinitionMemo.MemoID => _memoID;

        internal MemoBase() { }
    }

    public abstract class MemoBase<TRow, TComponent> : MemoBase, IIndexQuery<TRow>
        where TRow : class, IIndexableRow<TComponent>
        where TComponent : unmanaged, IEntityComponent
    {
        internal MemoBase() { }

        internal void Set<TIndex>(IndexedDB indexedDB, TIndex query)
            where TIndex : IIndexQuery<TRow>
        {
            indexedDB.ClearMemo(this);
            Union(indexedDB, query);
        }

        internal void Union<TIndex>(IndexedDB indexedDB, TIndex query)
            where TIndex : IIndexQuery<TRow>
        {
            using var results = indexedDB.Select<IndexableResultSet<TComponent>>().FromAll<TRow>().Where(query);

            foreach (var result in results)
            {
                var mapper = indexedDB.GetEGIDMapper(result.group);

                ref var originalGroupData = ref indexedDB.CreateOrGetMemoGroup(_memoID, result.group);

                foreach (var i in result.indices)
                {
                    originalGroupData.filter.Add(result.set.egid[i].ID.entityID, mapper);
                }
            }
        }

        internal void Intersect<TIndex>(IndexedDB indexedDB, TIndex other)
            where TIndex : IIndexQuery<TRow>
        {
            var originalData = GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (originalData == null)
                return;

            using var otherQuery = indexedDB.Select<IndexableResultSet<TComponent>>().FromAll<TRow>().Where(other);

            var otherData = otherQuery.Entities();

            var originalDataValues = originalData.GetValues(out var originalDataCount);

            for (int groupIndex = 0; groupIndex < originalDataCount; ++groupIndex)
            {
                ref var originalGroupData = ref originalDataValues[groupIndex];

                // if group is empty there is nothing to remove
                if (originalGroupData.filter.filteredIndices.Count() == 0)
                    continue;

                // if target is empty there is no intersection
                if (!otherData._config.temporaryGroups.ContainsKey(originalGroupData.groupID))
                    originalGroupData.filter.Clear();
            }

            foreach (var otherGroupData in otherData)
            {
                if (!originalData.ContainsKey(otherGroupData.group))
                    continue;

                ref var originalGroupData = ref originalData.GetValueByRef(otherGroupData.group);

                // if group is empty there is nothing to remove
                if (originalGroupData.filter.filteredIndices.Count() == 0)
                    continue;

                var indices = originalGroupData.filter.filteredIndices;

                for (int i = indices.Count() - 1; i >= 0; --i)
                {
                    var entityID = otherGroupData.set.egid[indices[i]].ID.entityID;

                    if (!otherGroupData.indices.Exists(entityID))
                        originalGroupData.filter.TryRemove(entityID);
                }
            }
        }

        internal IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB)
        {
            if (indexedDB.memos.TryGetValue(_memoID, out var result))
                return result.keyData;
            return default;
        }

        void IIndexQuery.Apply(ResultSetQueryConfig config)
        {
            config.indexers.Add(GetIndexerKeyData(config.indexedDB));
        }
    }
}

namespace Svelto.ECS.Schema.Definition
{
    // TODO soon IMemorableRow will be deprecated and all row can be memorable
    public interface IMemorableRow : IIndexableRow<EGIDComponent> { }

    public sealed class Memo<TRow> : MemoBase<TRow, EGIDComponent>
        where TRow : class, IMemorableRow
    { }
}