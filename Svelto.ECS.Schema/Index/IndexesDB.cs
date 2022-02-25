using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexesDB
    {
        // combined version of SchemaMetadata.groupToTable
        internal readonly FasterDictionary<ExclusiveGroupStruct, SchemaMetadata.TableNode> groupToTable;
        internal readonly HashSet<RefWrapperType> createdEngines;

        internal readonly FasterDictionary<int, IndexerData> indexers;
        internal readonly FasterDictionary<int, IndexerSetData> memos;

        // well... let's have some space for user defined filter
        private int filterIdCounter = 10000;

        internal EntitiesDB entitiesDB;

        internal IndexesDB()
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, SchemaMetadata.TableNode>();
            createdEngines = new HashSet<RefWrapperType>();

            indexers = new FasterDictionary<int, IndexerData>();
            memos = new FasterDictionary<int, IndexerSetData>();
        }

        internal void RegisterSchema(SchemaMetadata metadata)
        {
            groupToTable.Union(metadata.groupToTable);
        }

        internal bool TryGetShard(in ExclusiveGroupStruct group, out SchemaMetadata.ShardNode node)
        {
            if (!groupToTable.TryGetValue(group, out var table))
            {
                node = null;
                return false;
            }

            node = table.parent;
            return true;
        }

        // this should be fast enough, no group change means we don't have to rebuild filter
        internal void NotifyKeyUpdate<T>(ref IEntityIndexKey<T>.Component keyComponent, in T oldKey, in T newKey)
            where T : unmanaged, IEntityIndexKey<T>
        {
            // component updated but key didn't change
            if (oldKey.Equals(newKey))
                return;

            // index may exist but no table found
            if (!TryGetShard(keyComponent.ID.groupID, out var node))
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
                            UpdateFilters(indexer.IndexerID, ref keyComponent, oldKey, newKey);
                    }
                }

                node = node.parent;
            }
        }

        private void UpdateFilters<T>(int indexerId, ref IEntityIndexKey<T>.Component keyComponent, in T oldKey, in T newKey)
            where T : unmanaged, IEntityIndexKey<T>
        {
            ref var oldGroupData = ref CreateOrGetIndexerGroup(indexerId, oldKey, keyComponent.ID.groupID);
            ref var newGroupData = ref CreateOrGetIndexerGroup(indexerId, newKey, keyComponent.ID.groupID);

            var mapper = entitiesDB.QueryMappedEntities<IEntityIndexKey<T>.Component>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexerGroupData CreateOrGetIndexerGroup<T>(int indexerID, in T key, in ExclusiveGroupStruct groupID)
            where T : unmanaged, IEntityIndexKey<T>
        {
            var indexerData = CreateOrGetIndexerData<T>(indexerID);

            var groupDict = indexerData.CreateOrGet(key).groups;

            if (!groupDict.ContainsKey(groupID))
            {
                groupDict[groupID] = new IndexerGroupData
                {
                    group = groupID,
                    filter = entitiesDB.GetFilters()
                        .CreateOrGetFilterForGroup<IEntityIndexKey<T>.Component>(GenerateFilterId(), groupID)
                };
            }

            return ref groupDict.GetValueByRef(groupID);
        }

        private IndexerData<T> CreateOrGetIndexerData<T>(int indexerId)
            where T : unmanaged, IEntityIndexKey<T>
        {
            IndexerData<T> indexerData;

            if (!indexers.ContainsKey(indexerId))
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
