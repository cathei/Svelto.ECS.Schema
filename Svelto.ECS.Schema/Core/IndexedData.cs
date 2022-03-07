using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal struct IndexerGroupData : IEquatable<IndexerGroupData>
    {
        public IEntityTable table;
        public FilterGroup filter;

        public bool Equals(IndexerGroupData other) => table == other.table;
        public override int GetHashCode() => table.ExclusiveGroup.GetHashCode();
    }

    public struct IndexerKeyData
    {
        internal FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> groups;

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

    internal abstract class IndexerData { }

    internal sealed class IndexerData<TKey> : IndexerData
        where TKey : unmanaged, IEquatable<TKey>
    {
        private readonly FasterDictionary<KeyWrapper<TKey>, IndexerKeyData> keyToGroups
            = new FasterDictionary<KeyWrapper<TKey>, IndexerKeyData>();

        public ref IndexerKeyData CreateOrGet(in TKey key)
        {
            return ref keyToGroups.GetOrCreate(new KeyWrapper<TKey>(key), () => new IndexerKeyData
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>()
            });
        }

        public bool TryGetValue(in TKey key, out IndexerKeyData result)
        {
            return keyToGroups.TryGetValue(new KeyWrapper<TKey>(key), out result);
        }
    }

    internal sealed class MemoData
    {
        public readonly IndexerKeyData keyData;

        public MemoData()
        {
            keyData = new IndexerKeyData
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>()
            };
        }

        public void Clear()
        {
            keyData.Clear();
        }
    }
}
