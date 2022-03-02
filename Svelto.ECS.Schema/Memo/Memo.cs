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

    public class MemoBase : ISchemaDefinitionMemo
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _memoID = GlobalMemoCount.Generate();

        int ISchemaDefinitionMemo.MemoID => _memoID;

        internal MemoBase() { }
    }

    public class MemoBase<TR, TC> : MemoBase, IIndexQuery<TR>
        where TR : IEntityRow<TC>
        where TC : unmanaged, IEntityComponent, INeedEGID
    {
        internal MemoBase() { }

        public void Set<TQR, TQK, TQC>(IndexedDB indexedDB, IIndexQueryable<TQR, TQK, TQC> index, in TQK key)
            where TQR : IIndexableRow<TQK, TQC>, TR
            where TQK : unmanaged
            where TQC : unmanaged, IIndexableComponent<TQK>
        {
            Set<IndexQuery<TQR, TQK, TQC>, TQR>(indexedDB, index.Query(key));
        }

        public void Add(IndexedDB indexedDB, ref TC component)
        {
            indexedDB.AddMemo(this, component.ID.entityID, component.ID.groupID);
        }

        public void Remove(IndexedDB indexedDB, ref TC component)
        {
            indexedDB.RemoveMemo(this, component.ID.entityID, component.ID.groupID);
        }

        public void Clear(IndexedDB indexedDB)
        {
            indexedDB.ClearMemo(this);
        }

        public void Set(IndexedDB indexedDB, MemoBase other)
        {
            Set(indexedDB, other);
        }

        internal void Set<TQ, TQR>(IndexedDB indexedDB, TQ query)
            where TQ : IIndexQuery<TQR>
            where TQR : IEntityRow
        {
            Clear(indexedDB);
            Union<TQ, TQR>(indexedDB, query);
        }

        internal void Union<TQ, TQR>(IndexedDB indexedDB, TQ query)
            where TQ : IIndexQuery<TQR>
            where TQR : IEntityRow
        {
            var queryData = query.GetIndexedKeyData(indexedDB).groups;

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

                var mapper = indexedDB.entitiesDB.QueryMappedEntities<TC>(queryGroupData.table.ExclusiveGroup);

                // TODO: change group to table!
                var (components, _) = indexedDB.Select<TQR>().From(queryGroupData.table).Entities();

                ref var originalGroupData = ref indexedDB.CreateOrGetMemoGroup<TC>(_memoID, queryGroupData.table);

                foreach (var i in new IndexedIndices(queryGroupData.filter.filteredIndices))
                    originalGroupData.filter.Add(components[i].ID.entityID, mapper);
            }
        }

        internal void Intersect<TQ>(IndexedDB indexedDB, TQ query)
            where TQ : IIndexQuery
        {
            var originalData = GetIndexedKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (originalData == null)
                return;

            var queryData = query.GetIndexedKeyData(indexedDB).groups;

            // if empty nothing to intersect
            if (queryData == null)
            {
                Clear(indexedDB);
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
                if (!queryData.TryGetValue(originalDataValues[groupIndex].group, out var queryGroupData) ||
                    queryGroupData.filter.filteredIndices.Count() == 0)
                {
                    originalGroupData.filter.Clear();
                    continue;
                }

                // TODO: change group to table!
                // var components = indexedDB.Select<IMemorableRow>().From(queryGroupData.group).Entities();
                var (components, _) = indexedDB.entitiesDB.QueryEntities<EGIDComponent>(queryGroupData.group);

                // ugh I have to check what to delete
                // since I cannot change filter while iteration
                // this will be removed when Svelto updates it's filter system
                FasterList<uint> entityIDsToDelete = new FasterList<uint>();

                foreach (uint i in new IndexedIndices(originalGroupData.filter.filteredIndices))
                {
                    if (!queryGroupData.filter.Exists(components[i].ID.entityID))
                        entityIDsToDelete.Add(components[i].ID.entityID);
                }

                for (int i = 0; i < entityIDsToDelete.count; ++i)
                {
                    originalGroupData.filter.TryRemove(entityIDsToDelete[i]);
                }
            }
        }

        private IndexedKeyData<TR> GetIndexedKeyData(IndexedDB indexedDB)
        {
            indexedDB.memos.TryGetValue(_memoID, out var result);
            return result;
        }

        IndexedKeyData<TR> IIndexQuery<TR>.GetIndexedKeyData(IndexedDB indexedDB)
            => GetIndexedKeyData(indexedDB);
    }
}

namespace Svelto.ECS.Schema.Definition
{
    // we have EGIDComponent constraints because it requires INeedEGID
    // it won't be necessary when Svelto update it's filter utility functions
    public interface IMemorableRow : IEntityRow<EGIDComponent> { }

    public sealed class Memo<TRow> : MemoBase<TRow, EGIDComponent>
        where TRow : IMemorableRow
    {

    }
}