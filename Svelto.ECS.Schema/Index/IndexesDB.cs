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
        internal readonly HashSet<RefWrapperType> createdIndexerEngines;

        internal readonly FasterDictionary<int, IndexerData> indexers;
        internal readonly FasterDictionary<int, IndexerSetData> memos;

        internal readonly FasterList<ISchemaDefinitionIndex> stateMachineIndexers;

        // well... let's have some space for user defined filter
        private int filterIdCounter = 10000;

        internal EntitiesDB entitiesDB;

        internal IndexesDB()
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, SchemaMetadata.TableNode>();
            createdIndexerEngines = new HashSet<RefWrapperType>();

            indexers = new FasterDictionary<int, IndexerData>();
            memos = new FasterDictionary<int, IndexerSetData>();
            stateMachineIndexers = new FasterList<ISchemaDefinitionIndex>();
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
        internal void NotifyKeyUpdate<TK, TC>(ref TC keyComponent, in TK oldKey, in TK newKey)
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            // component updated but key didn't change
            if (oldKey.Equals(newKey))
                return;

            var keyType = TypeRefWrapper<TK>.wrapper;

            if (TryGetShard(keyComponent.ID.groupID, out var node))
            {
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

            // there wouldn't be too many indexers
            for (int i = 0; i < stateMachineIndexers.count; ++i)
            {
                var indexer = stateMachineIndexers[i];

                if (indexer.KeyType.Equals(keyType))
                    UpdateFilters(indexer.IndexerID, ref keyComponent, oldKey, newKey);
            }
        }

        internal void UpdateFilters<TK, TC>(int indexerId, ref TC keyComponent, in TK oldKey, in TK newKey)
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            ref var oldGroupData = ref CreateOrGetIndexerGroup<TK, TC>(indexerId, oldKey, keyComponent.ID.groupID);
            ref var newGroupData = ref CreateOrGetIndexerGroup<TK, TC>(indexerId, newKey, keyComponent.ID.groupID);

            var mapper = entitiesDB.QueryMappedEntities<TC>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexerGroupData CreateOrGetIndexerGroup<TK, TC>(int indexerID, in TK key, in ExclusiveGroupStruct groupID)
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            var indexerData = CreateOrGetIndexerData<TK>(indexerID);

            var groupDict = indexerData.CreateOrGet(key).groups;

            if (!groupDict.ContainsKey(groupID))
            {
                groupDict[groupID] = new IndexerGroupData
                {
                    group = groupID,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<TC>(GenerateFilterId(), groupID)
                };
            }

            return ref groupDict.GetValueByRef(groupID);
        }

        private IndexerData<TK> CreateOrGetIndexerData<TK>(int indexerId)
            where TK : unmanaged, IKeyEquatable<TK>
        {
            IndexerData<TK> indexerData;

            if (!indexers.ContainsKey(indexerId))
            {
                indexerData = new IndexerData<TK>();
                indexers[indexerId] = indexerData;
            }
            else
            {
                indexerData = (IndexerData<TK>)indexers[indexerId];
            }

            return indexerData;
        }

        private int GenerateFilterId()
        {
            return filterIdCounter++;
        }
    }
}
