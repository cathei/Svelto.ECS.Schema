using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal struct IndexerGroupData : IEquatable<IndexerGroupData>
    {
        public ExclusiveGroupStruct groupID;
        public FilterGroup filter;

        public bool Equals(IndexerGroupData other) => groupID == other.groupID;
        public override int GetHashCode() => groupID.GetHashCode();
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

    internal struct IndexerEntityData<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public EGID previousEGID;
        public TKey previousKey;
    }

    internal abstract class IndexerData
    {
        public abstract void RebuildFilters(in ExclusiveGroupStruct groupID, in EGIDMapper<RowIdentityComponent> mapper);
    }

    internal sealed class IndexerData<TKey> : IndexerData
        where TKey : unmanaged, IEquatable<TKey>
    {
        private readonly FasterDictionary<TKey, IndexerKeyData> keyToGroups
            = new FasterDictionary<TKey, IndexerKeyData>();

        public ref IndexerKeyData CreateOrGet(in TKey key)
        {
            return ref keyToGroups.GetOrCreate(key, () => new IndexerKeyData
            {
                groups = new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>()
            });
        }

        public bool TryGetValue(in TKey key, out IndexerKeyData result)
        {
            return keyToGroups.TryGetValue(key, out result);
        }

        public override void RebuildFilters(in ExclusiveGroupStruct groupID, in EGIDMapper<RowIdentityComponent> mapper)
        {
            var groupDatas = keyToGroups.GetValues(out var count);

            for (int i = 0; i < count; ++i)
            {
                if (groupDatas[i].groups.TryGetValue(groupID, out var groupData))
                {
                    groupData.filter.RebuildIndicesOnStructuralChange(mapper);
                }
            }
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
