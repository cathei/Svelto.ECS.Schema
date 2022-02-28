using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        // dictionary for each group
        private FasterDictionary<ExclusiveGroupStruct, GroupCache> _groupCaches
            = new FasterDictionary<ExclusiveGroupStruct, GroupCache>();

        internal class GroupCache
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
            if (_groupCaches.TryGetValue(groupID, out var groupCache) &&
                groupCache.componentToIndexers.TryGetValue(componentType, out var result))
            {
                return result;
            }

            if (groupCache == null)
            {
                groupCache = new GroupCache();
                _groupCaches.Add(groupID, groupCache);
            }

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentToIndexers = new FasterList<ISchemaDefinitionIndex>();

            groupCache.componentToIndexers.Add(componentType, componentToIndexers);

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
            ref var oldGroupData = ref CreateOrGetIndexedGroupData<TK, TC>(indexerId, oldKey, keyComponent.ID.groupID);
            ref var newGroupData = ref CreateOrGetIndexedGroupData<TK, TC>(indexerId, newKey, keyComponent.ID.groupID);

            var mapper = entitiesDB.QueryMappedEntities<TC>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexedGroupData CreateOrGetIndexedGroupData<TK, TC>(int indexerID, in TK key, in ExclusiveGroupStruct groupID)
            where TK : unmanaged
            where TC : unmanaged, IIndexedComponent<TK>
        {
            var indexerData = CreateOrGetIndexedData<TK>(indexerID);

            var groupDict = indexerData.CreateOrGet(key).groups;

            if (!groupDict.ContainsKey(groupID))
            {
                groupDict[groupID] = new IndexedGroupData
                {
                    group = groupID,
                    filter = entitiesDB.GetFilters().CreateOrGetFilterForGroup<TC>(GenerateFilterId(), groupID)
                };
            }

            return ref groupDict.GetValueByRef(groupID);
        }

        private IndexedData<TK> CreateOrGetIndexedData<TK>(int indexerId)
            where TK : unmanaged
        {
            IndexedData<TK> indexerData;

            if (!indexers.ContainsKey(indexerId))
            {
                indexerData = new IndexedData<TK>();
                indexers[indexerId] = indexerData;
            }
            else
            {
                indexerData = (IndexedData<TK>)indexers[indexerId];
            }

            return indexerData;
        }

        private int GenerateFilterId()
        {
            return filterIdCounter++;
        }
    }
}
