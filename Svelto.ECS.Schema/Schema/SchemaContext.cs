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

        internal struct IndexerData
        {
            public FasterDictionary<int, FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>> keyToGroups;
        }

        internal readonly IndexerData[] indexers;

        // well... let's have some space for user defined filter
        private int filterIdCounter = 10000;

        internal EntitiesDB entitiesDB;

        private readonly SchemaMetadata metadata;

        internal SchemaContext(SchemaMetadata metadata)
        {
            this.metadata = metadata;

            indexers = new IndexerData[metadata.indexerCount];
        }

        internal bool TryGetTable(ExclusiveGroupStruct group, out SchemaMetadata.TableNode table, out int offset)
        {
            if (!metadata.groupToTable.TryGetValue(group, out table))
            {
                offset = 0;
                return false;
            }

            offset = (int)(group.id - table.group.id);
            return true;
        }

        // this should be fast enough, no group change means we don't have to rebuild filter
        internal void NotifyKeyUpdate<T>(ref Indexed<T> keyComponent, int oldKey)
            where T : unmanaged, IEntityIndexKey
        {
            int newKey = keyComponent.Key;

            // component updated but key didn't change
            if (oldKey == newKey)
                return;

            // index may exist but no table found
            if (!TryGetTable(keyComponent.ID.groupID, out var table, out int offset))
                return;

            var node = table.parent;
            var keyType = typeof(T);

            while (node != null)
            {
                if (node.indexers != null)
                {
                    // there wouldn't be too many indexers
                    for (int i = 0; i < node.indexers.count; ++i)
                    {
                        var indexer = node.indexers[i];

                        if (indexer.keyType == keyType)
                        {
                            int indexerId = indexer.indexerStartIndex + (offset * node.groupSize / table.groupSize);
                            UpdateFilters(indexerId, ref keyComponent, oldKey, newKey);
                        }
                    }
                }

                node = node.parent;
            }
        }

        private void UpdateFilters<T>(int indexerId, ref Indexed<T> keyComponent, int oldKey, int newKey)
            where T : unmanaged, IEntityIndexKey
        {
            ref var oldGroupData = ref CreateOrGetGroupData<T>(indexerId, oldKey, keyComponent.ID.groupID);
            ref var newGroupData = ref CreateOrGetGroupData<T>(indexerId, newKey, keyComponent.ID.groupID);

            var mapper = entitiesDB.QueryMappedEntities<Indexed<T>>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private ref IndexerData CreateOrGetIndexerData(int indexerId)
        {
            ref var indexerData = ref indexers[indexerId];

            if (indexerData.keyToGroups == null)
            {
                indexerData.keyToGroups = new FasterDictionary<int, FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>>();
            }

            return ref indexerData;
        }

        internal ref IndexerGroupData CreateOrGetGroupData<T>(int indexerId, int key, ExclusiveGroupStruct group)
            where T : unmanaged, IEntityIndexKey
        {
            ref var indexerData = ref CreateOrGetIndexerData(indexerId);
            var groupDict = indexerData.keyToGroups.GetOrCreate(key, () => new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>());

            return ref groupDict.GetOrCreate(group, () => new IndexerGroupData
            {
                group = group,
                filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<Indexed<T>>(GenerateFilterId(), group)
            });
        }

        private int GenerateFilterId()
        {
            return filterIdCounter++;
        }
    }
}
