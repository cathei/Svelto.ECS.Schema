using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexesDB
    {
        // // cache for FindGroup
        // internal readonly FasterDictionary<RefWrapperType, FasterList<ExclusiveGroupStruct>> componentToGroup
        //     = new FasterDictionary<RefWrapperType, FasterList<ExclusiveGroupStruct>>();

        // dictionary for each group
        internal FasterDictionary<ExclusiveGroupStruct, GroupMetadata> groupMetadatas
            = new FasterDictionary<ExclusiveGroupStruct, GroupMetadata>();

        internal class GroupMetadata
        {
            // cache for indexer update
            public FasterDictionary<RefWrapperType, FasterList<ISchemaDefinitionIndex>> componentToIndexers
                = new FasterDictionary<RefWrapperType, FasterList<ISchemaDefinitionIndex>>();
        }

        public FasterList<ISchemaDefinitionIndex> FindIndexers<TK, TC>(in ExclusiveGroupStruct groupID)
            where TK : unmanaged
            where TC : IIndexedComponent<TK>
        {
            var componentType = TypeRefWrapper<TC>.wrapper;

            // cache exists?
            if (groupMetadatas.TryGetValue(groupID, out var groupMetadata) &&
                groupMetadata.componentToIndexers.TryGetValue(componentType, out var result))
            {
                return result;
            }

            if (groupMetadata == null)
            {
                groupMetadata = new GroupMetadata();
                groupMetadatas.Add(groupID, groupMetadata);
            }

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentToIndexers = new FasterList<ISchemaDefinitionIndex>();

            groupMetadata.componentToIndexers.Add(componentType, componentToIndexers);

            SchemaMetadata.ShardNode node = null;

            foreach (var schemaMetadata in registeredSchemas)
            {
                if (schemaMetadata.groupToTable.TryGetValue(groupID, out var table))
                {
                    node = table.parent;
                    break;
                }
            }

            while (node != null)
            {
                if (node.indexers != null)
                {
                    foreach (var indexer in node.indexers)
                    {
                        if (indexer.ComponentType.Equals(componentType))
                            componentToIndexers.Add(indexer);
                    }
                }

                node = node.parent;
            }

            foreach (var stateMachine in registeredStateMachines)
            {
                if (stateMachine.Index.ComponentType.Equals(componentType))
                    componentToIndexers.Add(stateMachine.Index);
            }

            return componentToIndexers;
        }

        private void UpdateFilters<TK, TC>(int indexerId, ref TC keyComponent, in TK oldKey, in TK newKey)
            where TK : unmanaged
            where TC : unmanaged, IIndexedComponent<TK>
        {
            ref var oldGroupData = ref CreateOrGetIndexerGroup<TK, TC>(indexerId, oldKey, keyComponent.ID.groupID);
            ref var newGroupData = ref CreateOrGetIndexerGroup<TK, TC>(indexerId, newKey, keyComponent.ID.groupID);

            var mapper = entitiesDB.QueryMappedEntities<TC>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexerGroupData CreateOrGetIndexerGroup<TK, TC>(int indexerID, in TK key, in ExclusiveGroupStruct groupID)
            where TK : unmanaged
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
            where TK : unmanaged
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
