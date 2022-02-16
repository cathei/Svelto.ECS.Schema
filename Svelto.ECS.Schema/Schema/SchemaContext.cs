using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public class SchemaContext
    {
        internal struct IndexerGroupData
        {
            public ExclusiveGroupStruct group;
            public FilterGroup filter;
        }

        internal abstract class IndexerData {}

        internal sealed class IndexerData<TKey> : IndexerData
            where TKey : unmanaged, IEntityIndexKey<TKey>
        {
            public readonly struct KeyWrapper : IEquatable<KeyWrapper>
            {
                private readonly TKey _value;
                private readonly int _hashcode;

                public KeyWrapper(TKey value)
                {
                    _value = value;
                    _hashcode = _value.GetHashCode();
                }

                // this uses IEntityIndexKey<T>.Equals
                public bool Equals(KeyWrapper other) => _value.Equals(other._value);

                public override bool Equals(object obj) => obj is IndexerData<TKey> other && Equals(other);

                public override int GetHashCode() => _hashcode;

                public static implicit operator TKey(KeyWrapper t) => t._value;
            }

            private readonly FasterDictionary<KeyWrapper, FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>> keyToGroups
                = new FasterDictionary<KeyWrapper, FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>>();

            public FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> CreateOrGet(in TKey key)
            {
                return keyToGroups.GetOrCreate(new KeyWrapper(key), () => new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>());
            }

            public bool TryGetValue(in TKey key, out FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> result)
            {
                return keyToGroups.TryGetValue(new KeyWrapper(key), out result);
            }
        }

        internal readonly FasterDictionary<int, IndexerData> indexers;

        // well... let's have some space for user defined filter
        private int filterIdCounter = 10000;

        internal EntitiesDB entitiesDB;

        private readonly SchemaMetadata metadata;

        internal SchemaContext(SchemaMetadata metadata)
        {
            this.metadata = metadata;

            indexers = new FasterDictionary<int, IndexerData>();
        }

        internal bool TryGetPartition(in ExclusiveGroupStruct group, out SchemaMetadata.Node partition)
        {
            return metadata.groupToParentPartition.TryGetValue(group, out partition);
        }

        // this should be fast enough, no group change means we don't have to rebuild filter
        internal void NotifyKeyUpdate<T>(ref Indexed<T> keyComponent, in T oldKey, in T newKey)
            where T : unmanaged, IEntityIndexKey<T>
        {
            // component updated but key didn't change
            if (oldKey.Equals(newKey))
                return;

            // index may exist but no table found
            if (!TryGetPartition(keyComponent.ID.groupID, out var node))
                return;

            var keyType = TypeRefWrapper<T>.wrapper;

            while (node != null)
            {
                if (node.indexers != null)
                {
                    // there wouldn't be too many indexers
                    for (int i = 0; i < node.indexers.count; ++i)
                    {
                        var indexer = node.indexers[i];

                        if (indexer.KeyType.Equals(keyType))
                            UpdateFilters(indexer.IndexerId, ref keyComponent, oldKey, newKey);
                    }
                }

                node = node.parent;
            }
        }

        private void UpdateFilters<T>(int indexerId, ref Indexed<T> keyComponent, in T oldKey, in T newKey)
            where T : unmanaged, IEntityIndexKey<T>
        {
            ref var oldGroupData = ref CreateOrGetGroupData(indexerId, oldKey, keyComponent.ID.groupID);
            ref var newGroupData = ref CreateOrGetGroupData(indexerId, newKey, keyComponent.ID.groupID);

            var mapper = entitiesDB.QueryMappedEntities<Indexed<T>>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexerGroupData CreateOrGetGroupData<T>(int indexerId, in T key, ExclusiveGroupStruct group)
            where T : unmanaged, IEntityIndexKey<T>
        {
            var indexerData = CreateOrGetIndexerData<T>(indexerId);

            var groupDict = indexerData.CreateOrGet(key);

            return ref groupDict.GetOrCreate(group, () => new IndexerGroupData
            {
                group = group,
                filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<Indexed<T>>(GenerateFilterId(), group)
            });
        }

        private IndexerData<T> CreateOrGetIndexerData<T>(int indexerId)
            where T : unmanaged, IEntityIndexKey<T>
        {
            IndexerData<T> indexerData;

            if (indexers.ContainsKey(indexerId))
            {
                indexerData = new IndexerData<T>();
                indexers[indexerId] = indexerData;
            }
            else
            {
                indexerData = (IndexerData<T>)indexers[indexerId];
            }

            return indexerData;
        }

        private int GenerateFilterId()
        {
            return filterIdCounter++;
        }
    }
}
