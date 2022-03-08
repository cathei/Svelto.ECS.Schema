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
        where TComponent : unmanaged, IEntityComponent, INeedEGID
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
            var queryData = query.GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to add
            if (queryData == null)
                return;

            var queryDataValues = queryData.GetValues(out var queryDataCount);

            for (int groupIndex = 0; groupIndex < queryDataCount; ++groupIndex)
            {
                var queryGroupData = queryDataValues[groupIndex];

                // if empty nothing to add
                if (queryGroupData.filter.filteredIndices.Count() == 0)
                    continue;

                var table = indexedDB.FindTable<TRow>(queryGroupData.groupID);

                // type mismatch - noathing to add
                if (table == null)
                    continue;

                var mapper = indexedDB.GetEGIDMapper(queryGroupData.groupID);

                // TODO: change group to table!
                var result = indexedDB.Select<IndexableResultSet<TComponent>>().From(table).Entities();

                ref var originalGroupData = ref indexedDB.CreateOrGetMemoGroup(_memoID, table);

                foreach (var i in new IndexedIndices(queryGroupData.filter.filteredIndices))
                    originalGroupData.filter.Add(result.set.component[i].ID.entityID, mapper);
            }
        }

        internal void Intersect<TIndex>(IndexedDB indexedDB, TIndex query)
            where TIndex : IIndexQuery<TRow>
        {
            var originalData = GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (originalData == null)
                return;

            var queryData = query.GetIndexerKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (queryData == null)
            {
                indexedDB.ClearMemo(this);
                return;
            }

            var originalDataValues = originalData.GetValues(out var originalDataCount);

            for (int groupIndex = 0; groupIndex < originalDataCount; ++groupIndex)
            {
                ref var originalGroupData = ref originalDataValues[groupIndex];

                // if group is empty there is nothing to remove
                if (originalGroupData.filter.filteredIndices.Count() == 0)
                    continue;

                // if target is empty there is no intersection
                if (!queryData.TryGetValue(originalDataValues[groupIndex].groupID, out var queryGroupData) ||
                    queryGroupData.filter.filteredIndices.Count() == 0)
                {
                    originalGroupData.filter.Clear();
                    continue;
                }

                var table = indexedDB.FindTable<TRow>(queryGroupData.groupID);

                // type mismatch - no intersection
                if (table == null)
                {
                    originalGroupData.filter.Clear();
                    continue;
                }

                var result = indexedDB.Select<IndexableResultSet<TComponent>>().From(table).Entities();

                // ugh I have to check what to delete
                // since I cannot change filter while iteration
                // this will be removed when Svelto updates it's filter system
                FasterList<uint> entityIDsToDelete = new FasterList<uint>();

                foreach (uint i in new IndexedIndices(originalGroupData.filter.filteredIndices))
                {
                    ref var component = ref result.set.component[i];

                    if (!queryGroupData.filter.Exists(component.ID.entityID))
                        entityIDsToDelete.Add(component.ID.entityID);
                }

                for (int i = 0; i < entityIDsToDelete.count; ++i)
                {
                    originalGroupData.filter.TryRemove(entityIDsToDelete[i]);
                }
            }
        }

        private IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB)
        {
            if (indexedDB.memos.TryGetValue(_memoID, out var result))
                return result.keyData;
            return default;
        }

        IndexerKeyData IIndexQuery.GetIndexerKeyData(IndexedDB indexedDB)
            => GetIndexerKeyData(indexedDB);
    }
}

namespace Svelto.ECS.Schema.Definition
{
    public interface IMemorableRow : IIndexableRow<EGIDComponent> { }

    public sealed class Memo<TRow> : MemoBase<TRow, EGIDComponent>
        where TRow : class, IMemorableRow
    { }
}