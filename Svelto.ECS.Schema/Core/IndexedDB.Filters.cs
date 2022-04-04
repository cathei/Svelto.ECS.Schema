using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        // dictionary for each group
        private readonly FasterDictionary<RefWrapperType, IndexableComponentCache> _componentCaches = new();

        internal class IndexableComponentCache
        {
            // cached when called
            public FasterList<IEntityIndex> indexers = null;
        }

        // cache for indexer update
        internal class IndexableComponentCache<TKey> : IndexableComponentCache
            where TKey : unmanaged, IEquatable<TKey>
        {
            // we have own structure to track previous state of indexed component
            public SharedSveltoDictionaryNative<EntityReference, TKey> previousKeys = new(0);
        }

        internal Memo<IPrimaryKeyRow> entitiesToUpdateGroup = new Memo<IPrimaryKeyRow>();

        internal IndexableComponentCache<TK> CreateOrGetComponentCache<TK>(in RefWrapperType componentType)
            where TK : unmanaged, IEquatable<TK>
        {
            return (IndexableComponentCache<TK>)_componentCaches.GetOrAdd(
                componentType, () => new IndexableComponentCache<TK>());
        }

        internal FasterList<IEntityIndex> FindIndexers(
            in RefWrapperType componentType, IndexableComponentCache componentCache)
        {
            if (componentCache.indexers != null)
                return componentCache.indexers;

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentIndexers = new FasterList<IEntityIndex>();

            foreach (var schemaMetadata in registeredSchemas)
            {
                foreach (var indexer in schemaMetadata.indexers)
                {
                    if (indexer.ComponentType.Equals(componentType))
                        componentIndexers.Add(indexer);
                }
            }

            componentCache.indexers = componentIndexers;
            return componentIndexers;
        }

        // remove
        internal void RemoveIndexableComponent<TK>(RefWrapperType componentType, in EGID egid)
            where TK : unmanaged, IEquatable<TK>
        {
            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);
            var componentCache = CreateOrGetComponentCache<TK>(componentType);

            // persistent filters will be cleared automatically, we need to remove cache tho
            componentCache.previousKeys.Remove(entityReference);
        }

        // add or update
        internal void UpdateIndexableComponent<TK>(RefWrapperType componentType, in EGID egid, in TK key)
            where TK : unmanaged, IEquatable<TK>
        {
            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);
            var componentCache = CreateOrGetComponentCache<TK>(componentType);

            // has previous record
            if (componentCache.previousKeys.TryGetValue(entityReference, out var previousKey))
            {
                if (previousKey.Equals(key))
                {
                    // no changes, nothing to update
                    return;
                }

                var oldIndexers = FindIndexers(componentType, componentCache);

                foreach (var indexer in oldIndexers)
                {
                    // ref var oldGroupData = ref CreateOrGetIndexedGroupData(
                    //     indexer.IndexerID, entityData.previousKey, entityData.previousEGID.groupID);

                    var filter = oldIndexers.GetFilter();

                    oldGroupData.filter.Remove(entityData.previousEGID.entityID);
                }


                // if (entityData.previousEGID.Equals(egid) &&
                //     entityData.previousKey.Equals(key))
                // {
                //     // no changes, nothing to update
                //     return;
                // }

                // var oldIndexers = FindIndexers(componentType, componentCache, entityData.previousEGID.groupID);

                // foreach (var indexer in oldIndexers)
                // {
                //     ref var oldGroupData = ref CreateOrGetIndexedGroupData(
                //         indexer.IndexerID, entityData.previousKey, entityData.previousEGID.groupID);

                //     oldGroupData.filter.Remove(entityData.previousEGID.entityID);
                // }
            }

            // update record
            entityData.previousEGID = egid;
            entityData.previousKey = key;
            componentCache.entities[entityReference] = entityData;

            var indexers = FindIndexers(componentType, componentCache, egid.groupID);

            if (indexers.count > 0)
            {
                var mapper = GetEGIDMapper(egid.groupID);

                foreach (var indexer in indexers)
                {
                    ref var newGroupData = ref CreateOrGetIndexedGroupData(
                        indexer.IndexerID, key, egid.groupID);

                    newGroupData.filter.Add(egid.entityID, mapper);
                }
            }

            this.Memo(entitiesToUpdateGroup).Add(egid.entityID, egid.groupID);
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
