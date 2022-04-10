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
            public FasterList<IIndexDefinition> indexers = null;
        }

        // cache for indexer update
        internal class IndexableComponentCache<TKey> : IndexableComponentCache
            where TKey : unmanaged, IEquatable<TKey>
        {
            // we have own structure to track previous state of indexed component
            public SharedSveltoDictionaryNative<EntityReference, TKey> previousKeys = new(0);
        }

        // this will be cleared every time entity submission happens
        // that means if user has any Update on it's iteration,
        // they need to call IndexedDB.Step() before entity submission
        internal Memo<IPrimaryKeyRow> entitiesToUpdateGroup = new();

        internal IndexableComponentCache<TK> GetOrAddComponentCache<TK>(in RefWrapperType componentType)
            where TK : unmanaged, IEquatable<TK>
        {
            return (IndexableComponentCache<TK>)_componentCaches.GetOrAdd(
                componentType, () => new IndexableComponentCache<TK>());
        }

        internal FasterList<IIndexDefinition> FindIndexers(
            in RefWrapperType componentType, IndexableComponentCache componentCache)
        {
            if (componentCache.indexers != null)
                return componentCache.indexers;

            // Cache doesn't exists, let's build one
            // We don't support dynamic addition of Schemas and StateMachines
            var componentIndexers = new FasterList<IIndexDefinition>();

            foreach (var schemaMetadata in registeredSchemas)
            {
                foreach (var indexer in schemaMetadata.indexers)
                {
                    if (indexer.ComponentType.Equals(componentType))
                        componentIndexers.Add(indexer);
                }
            }

            foreach (var stateMachine in registeredStateMachines)
            {
                var indexer = stateMachine.Index;

                if (indexer.ComponentType.Equals(componentType))
                    componentIndexers.Add(indexer);
            }

            componentCache.indexers = componentIndexers;
            return componentIndexers;
        }

        private void UpdateFilterInternal<TK>(RefWrapperType componentType,
                IndexableComponentCache<TK> componentCache,
                in EGID egid, in EntityReference entityReference, in TK key)
            where TK : unmanaged, IEquatable<TK>
        {
            var indexers = FindIndexers(componentType, componentCache);

            // has previous record
            if (componentCache.previousKeys.TryGetValue(entityReference, out var previousKey))
            {
                if (previousKey.Equals(key))
                {
                    // no changes, nothing to update
                    return;
                }

                foreach (var indexer in indexers)
                {
                    ref var filter = ref GetFilter(indexer.IndexerID, previousKey);

                    filter.Remove(egid);
                }
            }

            // update record
            componentCache.previousKeys[entityReference] = key;

            if (indexers.count > 0)
            {
                var mapper = GetNativeEGIDMapper(egid.groupID);

                foreach (var indexer in indexers)
                {
                    ref var filter = ref GetFilter(indexer.IndexerID, key);

                    filter.Add(egid, mapper);
                }
            }
        }

        // remove
        internal void RemoveIndexableComponent<TK>(RefWrapperType componentType, in EntityReference entityReference)
            where TK : unmanaged, IEquatable<TK>
        {
            var componentCache = GetOrAddComponentCache<TK>(componentType);

            // persistent filters will be cleared automatically, we need to remove cache tho
            componentCache.previousKeys.Remove(entityReference);
        }

        // add or update
        internal void UpdateIndexableComponent<TK>(RefWrapperType componentType, in EGID egid, in TK key)
            where TK : unmanaged, IEquatable<TK>
        {
            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);
            var componentCache = GetOrAddComponentCache<TK>(componentType);

            UpdateFilterInternal(componentType, componentCache, egid, entityReference, key);

            this.Memo(entitiesToUpdateGroup).Add(egid.entityID, egid.groupID);
        }

        internal ref EntityFilterCollection GetFilter<TKey>(FilterContextID indexerID, TKey key)
            where TKey : unmanaged, IEquatable<TKey>
        {
            var data = indexers.GetOrAdd(indexerID.id, () => new IndexerData<TKey>()) as IndexerData<TKey>;
            var filterID = new CombinedFilterID(data.Get(key), indexerID);

            return ref GetOrAddPersistentFilter(filterID);
        }
    }
}
