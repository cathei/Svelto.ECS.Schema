using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        // dictionary for each group
        private readonly FasterDictionary<RefWrapperType, IndexableComponentCache> _componentCaches
            = new FasterDictionary<RefWrapperType, IndexableComponentCache>();

        internal class IndexableComponentCache
        {
            public FasterDictionary<ExclusiveGroupStruct, FasterList<IEntityIndex>> groupToIndexers
                = new FasterDictionary<ExclusiveGroupStruct, FasterList<IEntityIndex>>();
        }

        // cache for indexer update
        internal class IndexableComponentCache<TKey> : IndexableComponentCache
            where TKey : unmanaged, IEquatable<TKey>
        {
            // we have own structure to track previous state of indexed component
            public SveltoDictionaryNative<EntityReference, IndexerEntityData<TKey>> entities
                = new SveltoDictionaryNative<EntityReference, IndexerEntityData<TKey>>(0);
        }

        internal IndexableComponentCache<TK> CreateOrGetComponentCache<TK>(in RefWrapperType componentType)
            where TK : unmanaged, IEquatable<TK>
        {
            return (IndexableComponentCache<TK>)_componentCaches.GetOrCreate(
                componentType, () => new IndexableComponentCache<TK>());
        }

        internal FasterList<IEntityIndex> FindIndexers(
            in RefWrapperType componentType, IndexableComponentCache componentCache, in ExclusiveGroupStruct groupID)
        {
            if (componentCache.groupToIndexers.TryGetValue(groupID, out var result))
                return result;

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentIndexers = new FasterList<IEntityIndex>();

            componentCache.groupToIndexers.Add(groupID, componentIndexers);

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

        /// <summary>
        /// We need to rebuild all groups that has structural change
        /// This can be safely removed as new Filters system applied
        /// </summary>
        internal void RebuildFilters(HashSet<ExclusiveGroupStruct> groups)
        {
            foreach (var group in groups)
            {
                var mapper = GetEGIDMapper(group);

                var indexerValues = indexers.GetValues(out var count);

                for (int i = 0; i < count; ++i)
                {
                    indexerValues[i].RebuildFilters(group, mapper);
                }
            }
        }

        // remove
        internal void RemoveIndexableComponent<TK>(RefWrapperType componentType, in EGID egid)
            where TK : unmanaged, IEquatable<TK>
        {
            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);
            var componentCache = CreateOrGetComponentCache<TK>(componentType);

            if (componentCache.entities.TryGetValue(entityReference, out var entityData))
            {
                // remove old indexers
                var oldIndexers = FindIndexers(componentType, componentCache, entityData.previousEGID.groupID);

                foreach (var indexer in oldIndexers)
                {
                    ref var oldGroupData = ref CreateOrGetIndexedGroupData(
                        indexer.IndexerID, entityData.previousKey, entityData.previousEGID.groupID);

                    oldGroupData.filter.Remove(entityData.previousEGID.entityID);
                }

                // remove reference entry
                componentCache.entities.Remove(entityReference);
            }
        }

        // add or update
        internal void UpdateIndexableComponent<TK>(RefWrapperType componentType, in EGID egid, in TK key)
            where TK : unmanaged, IEquatable<TK>
        {
            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);
            var componentCache = CreateOrGetComponentCache<TK>(componentType);

            // has previous record
            if (componentCache.entities.TryGetValue(entityReference, out var entityData))
            {
                if (entityData.previousEGID.Equals(egid) &&
                    entityData.previousKey.Equals(key))
                {
                    // no changes, nothing to update
                    return;
                }

                var oldIndexers = FindIndexers(componentType, componentCache, entityData.previousEGID.groupID);

                foreach (var indexer in oldIndexers)
                {
                    ref var oldGroupData = ref CreateOrGetIndexedGroupData(
                        indexer.IndexerID, entityData.previousKey, entityData.previousEGID.groupID);

                    oldGroupData.filter.Remove(entityData.previousEGID.entityID);
                }
            }

            // update record
            entityData.previousEGID = egid;
            entityData.previousKey = key;
            componentCache.entities[entityReference] = entityData;

            var indexers = FindIndexers(componentType, componentCache, egid.groupID);
            var mapper = GetEGIDMapper(egid.groupID);

            foreach (var indexer in indexers)
            {
                ref var newGroupData = ref CreateOrGetIndexedGroupData(
                    indexer.IndexerID, key, egid.groupID);

                newGroupData.filter.Add(egid.entityID, mapper);
            }
        }

        internal ref IndexerGroupData CreateOrGetIndexedGroupData<TK>(int indexerID, in TK key, in ExclusiveGroupStruct groupID)
            where TK : unmanaged, IEquatable<TK>
        {
            var indexerData = CreateOrGetIndexedData<TK>(indexerID);

            var groupDict = indexerData.CreateOrGet(key).groups;

            if (!groupDict.ContainsKey(groupID))
            {
                groupDict[groupID] = new IndexerGroupData
                {
                    groupID = groupID,
                    filter = entitiesDB.GetFilters()
                        .CreateOrGetFilterForGroup<RowIdentityComponent>(GenerateFilterId(), groupID)
                };
            }

            return ref groupDict.GetValueByRef(groupID);
        }

        private IndexerData<TK> CreateOrGetIndexedData<TK>(int indexerId)
            where TK : unmanaged, IEquatable<TK>
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
