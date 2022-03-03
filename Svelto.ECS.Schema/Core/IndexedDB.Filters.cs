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

        // cache for indexer update
        internal struct GroupCache
        {
            public FasterDictionary<RefWrapperType, FasterList<ISchemaDefinitionIndex>> componentToIndexers;
        }

        internal FasterList<ISchemaDefinitionIndex> FindIndexers<TK, TC>(in ExclusiveGroupStruct groupID)
            where TK : unmanaged
            where TC : IIndexableComponent<TK>
        {
            var componentType = TypeRefWrapper<TC>.wrapper;

            var groupCache = _groupCaches.GetOrCreate(groupID, () => new GroupCache
            {
                componentToIndexers = new FasterDictionary<RefWrapperType, FasterList<ISchemaDefinitionIndex>>()
            });

            if (groupCache.componentToIndexers.TryGetValue(componentType, out var result))
                return result;

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentIndexers = new FasterList<ISchemaDefinitionIndex>();

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

        private void UpdateFilters<TR, TK, TC>(int indexerId, ref TC keyComponent, in TK oldKey, in TK newKey)
            where TR : IIndexableRow<TK, TC>
            where TK : unmanaged
            where TC : unmanaged, IIndexableComponent<TK>
        {
            var table = FindTable<TR>(keyComponent.ID.groupID);

            if (table == null)
                return;

            ref var oldGroupData = ref CreateOrGetIndexedGroupData<TR, TK, TC>(indexerId, oldKey, table);
            ref var newGroupData = ref CreateOrGetIndexedGroupData<TR, TK, TC>(indexerId, newKey, table);

            var mapper = entitiesDB.QueryMappedEntities<TC>(keyComponent.ID.groupID);

            oldGroupData.filter.TryRemove(keyComponent.ID.entityID);
            newGroupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        internal ref IndexedGroupData<TR> CreateOrGetIndexedGroupData<TR, TK, TC>(int indexerID, in TK key, IEntityTable<TR> table)
            where TR : IIndexableRow<TK, TC>
            where TK : unmanaged
            where TC : unmanaged, IIndexableComponent<TK>
        {
            var indexerData = CreateOrGetIndexedData<TR, TK>(indexerID);

            var groupDict = indexerData.CreateOrGet(key).groups;

            if (!groupDict.ContainsKey(table.ExclusiveGroup))
            {
                groupDict[table.ExclusiveGroup] = new IndexedGroupData<TR>
                {
                    table = table,
                    filter = entitiesDB.GetFilters()
                        .CreateOrGetFilterForGroup<TC>(GenerateFilterId(), table.ExclusiveGroup)
                };
            }

            return ref groupDict.GetValueByRef(table.ExclusiveGroup);
        }

        private IndexedData<TR, TK> CreateOrGetIndexedData<TR, TK>(int indexerId)
            where TR : IEntityRow
            where TK : unmanaged
        {
            IndexedData<TR, TK> indexerData;

            if (!indexers.ContainsKey(indexerId))
            {
                indexerData = new IndexedData<TR, TK>();
                indexers[indexerId] = indexerData;
            }
            else
            {
                indexerData = (IndexedData<TR, TK>)indexers[indexerId];
            }

            return indexerData;
        }

        private int GenerateFilterId()
        {
            return filterIdCounter++;
        }
    }
}
