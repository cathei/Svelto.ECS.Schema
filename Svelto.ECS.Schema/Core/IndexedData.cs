using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal struct IndexedGroupData<TRow> : IEquatable<IndexedGroupData<TRow>>
        where TRow : IEntityRow
    {
        public IEntityTable<TRow> table;
        public FilterGroup filter;

        public bool Equals(IndexedGroupData<TRow> other) => table == other.table;
        public override int GetHashCode() => table.ExclusiveGroup.GetHashCode();
    }

    public struct IndexedKeyData<TRow> where TRow : IEntityRow
    {
        internal FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TRow>> groups;

        // Do I want to delete group from dictionary, but only cache filter id?
        // Can be rewarded in faster iteration
        // internal FasterDictionary<ExclusiveGroupStruct, int> filterIDs;

        public void Clear()
        {
            var values = groups.GetValues(out var count);

            for (int i = 0; i < count; ++i)
                values[i].filter.Clear();
        }
    }

    internal interface IIndexedData { }

    internal sealed class IndexedData<TRow, TKey> : IIndexedData
        where TRow : IEntityRow
        where TKey : unmanaged
    {
        private readonly FasterDictionary<KeyWrapper<TKey>, IndexedKeyData<TRow>> keyToGroups
            = new FasterDictionary<KeyWrapper<TKey>, IndexedKeyData<TRow>>();

        public ref IndexedKeyData<TRow> CreateOrGet(in TKey key)
        {
            return ref keyToGroups.GetOrCreate(new KeyWrapper<TKey>(key), () => new IndexedKeyData<TRow>
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TRow>>()
            });
        }

        public bool TryGetValue(in TKey key, out IndexedKeyData<TRow> result)
        {
            return keyToGroups.TryGetValue(new KeyWrapper<TKey>(key), out result);
        }
    }

    internal interface IMemoData
    {
        void Clear();
    }

    internal sealed class MemoData<TRow> : IMemoData
        where TRow : IEntityRow
    {
        public readonly IndexedKeyData<TRow> keyData;

        public MemoData()
        {
            keyData = new IndexedKeyData<TRow>
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TRow>>()
            };
        }

        public void Clear()
        {
            keyData.Clear();
        }
    }
}
