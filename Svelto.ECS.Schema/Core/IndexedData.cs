using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal struct IndexedGroupData
    {
        public ExclusiveGroupStruct group;
        public FilterGroup filter;

        public override int GetHashCode() => group.GetHashCode();
    }

    public struct IndexedKeyData
    {
        internal FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> groups;

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

    internal abstract class IndexedData { }

    internal sealed class IndexedData<TKey> : IndexedData
        where TKey : unmanaged
    {
        private readonly FasterDictionary<KeyWrapper<TKey>, IndexedKeyData> keyToGroups
            = new FasterDictionary<KeyWrapper<TKey>, IndexedKeyData>();

        public ref IndexedKeyData CreateOrGet(in TKey key)
        {
            return ref keyToGroups.GetOrCreate(new KeyWrapper<TKey>(key), () => new IndexedKeyData
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexedGroupData>()
            });
        }

        public bool TryGetValue(in TKey key, out IndexedKeyData result)
        {
            return keyToGroups.TryGetValue(new KeyWrapper<TKey>(key), out result);
        }
    }
}
