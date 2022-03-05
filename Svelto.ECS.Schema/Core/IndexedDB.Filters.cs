using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        // dictionary for each group
        private FasterDictionary<ExclusiveGroupStruct, GroupCache> _groupCaches
            = new FasterDictionary<ExclusiveGroupStruct, GroupCache>();

        // cache for indexer update
        internal struct GroupCache
        {
            public FasterDictionary<RefWrapperType, FasterList<IEntityIndex>> componentToIndexers;
        }

        internal FasterList<IEntityIndex> FindIndexers<TK, TC>(in ExclusiveGroupStruct groupID)
            where TK : unmanaged
        {
            var componentType = TypeRefWrapper<TC>.wrapper;

            var groupCache = _groupCaches.GetOrCreate(groupID, () => new GroupCache
            {
                componentToIndexers = new FasterDictionary<RefWrapperType, FasterList<IEntityIndex>>()
            });

            if (groupCache.componentToIndexers.TryGetValue(componentType, out var result))
                return result;

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentIndexers = new FasterList<IEntityIndex>();

            groupCache.componentToIndexers.Add(componentType, componentIndexers);

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
                            componentIndexers.Add(indexer);
                    }
                }

                node = node.parent;
            }

            foreach (var stateMachine in registeredStateMachines)
            {
                if (stateMachine.Index.ComponentType.Equals(componentType))
                    componentIndexers.Add(stateMachine.Index);
            }

            return componentIndexers;
        }

        private void UpdateFilters<TK, TC>(int indexerId, ref TC keyComponent, in TK oldKey, in TK newKey)
            where TK : unmanaged
            where TC : struct, IIndexedComponent
        {
            var table = FindTable<IIndexableRow<TC>>(keyComponent.ID.groupID);

            if (table == null)
                return;

            ref var oldGroupData = ref CreateOrGetIndexedGroupData(indexerId, oldKey, table);
            ref var newGroupData = ref CreateOrGetIndexedGroupData(indexerId, newKey, table);

            var mapper = entitiesDB.QueryMappedEntities<RowIdentityComponent>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexerGroupData CreateOrGetIndexedGroupData<TK>(int indexerID, in TK key, IEntityTable table)
            where TK : unmanaged
        {
            var indexerData = CreateOrGetIndexedData<TK>(indexerID);

            var groupDict = indexerData.CreateOrGet(key).groups;

            if (!groupDict.ContainsKey(table.ExclusiveGroup))
            {
                groupDict[table.ExclusiveGroup] = new IndexerGroupData
                {
                    table = table,
                    filter = entitiesDB.GetFilters()
                        .CreateOrGetFilterForGroup<RowIdentityComponent>(GenerateFilterId(), table.ExclusiveGroup)
                };
            }

            return ref groupDict.GetValueByRef(table.ExclusiveGroup);
        }

        private IndexerData<TK> CreateOrGetIndexedData<TK>(int indexerId)
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
