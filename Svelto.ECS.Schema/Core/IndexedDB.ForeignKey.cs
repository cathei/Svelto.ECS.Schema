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
        internal class ForeignKeyCache
        {
            public FasterDictionary<EntityReference, HashSet<EntityReference>> reverseForiegnKey = new();
            public FasterDictionary<EntityReference, EntityReference> previousForeignKey = new();
        }

        private readonly FasterDictionary<RefWrapperType, ForeignKeyCache> _foreignKeyCache = new();

        internal ForeignKeyCache GetOrAddForeignKeyCache(RefWrapperType foreignKeyType)
        {
            return _foreignKeyCache.GetOrAdd(foreignKeyType, () => new());
        }

        private void RemoveFromFilterInternal(RefWrapperType fkType,
            IndexableComponentCache<ExclusiveGroupStruct> componentCache,
            in EGID egid, in EntityReference entityReference)
        {
            var indexers = FindIndexers(fkType, componentCache);

            // has previous record
            if (componentCache.previousKeys.TryGetValue(entityReference, out var previousKey))
            {
                foreach (var indexer in indexers)
                {
                    ref var filter = ref GetFilter(indexer.IndexerID, previousKey);

                    filter.Remove(egid);
                }

                componentCache.previousKeys.Remove(entityReference);
            }
        }

        private void RemoveForeignKeyFromCacheInternal(RefWrapperType fkType,
            in EntityReference entityReference)
        {
            var fkCache = GetOrAddForeignKeyCache(fkType);

            if (fkCache.previousForeignKey.TryGetValue(entityReference, out var other))
            {
                fkCache.previousForeignKey.Remove(entityReference);

                if (fkCache.reverseForiegnKey.TryGetValue(other, out var otherHashSet))
                    otherHashSet.Remove(entityReference);
            }
        }

        internal void UpdateForeignKeyComponent<TComponent>(in EGID egid, in EntityReference other)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            var fkType = ForeignKeyTag<TComponent>.wrapper;

            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);

            var componentCache = GetOrAddComponentCache<ExclusiveGroupStruct>(fkType);
            var fkCache = GetOrAddForeignKeyCache(fkType);

            if (fkCache.previousForeignKey.TryGetValue(entityReference, out var previousReference) &&
                fkCache.reverseForiegnKey.TryGetValue(previousReference, out var previousHashSet))
            {
                previousHashSet.Remove(entityReference);
            }

            fkCache.previousForeignKey[entityReference] = other;

            var otherHashSet = fkCache.reverseForiegnKey.GetOrAdd(other, () => new());
            otherHashSet.Add(entityReference);

            if (other == EntityReference.Invalid || !TryGetEGID(other, out var otherID) ||
                FindTable<IReferenceableRow<TComponent>>(otherID.groupID) == null)
            {
                RemoveFromFilterInternal(fkType, componentCache, egid, entityReference);
            }
            else
            {
                UpdateFilterInternal(fkType, componentCache, egid, entityReference, otherID.groupID);
            }
        }

        internal void RemoveForeignKeyComponent<TComponent>(in EGID egid)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            var fkType = ForeignKeyTag<TComponent>.wrapper;

            // we need to compare with previous key with reference because it's only reliable value
            var entityReference = entitiesDB.GetEntityReference(egid);
            var componentCache = GetOrAddComponentCache<ExclusiveGroupStruct>(fkType);

            // persistent filters will be cleared automatically, we need to remove cache tho
            componentCache.previousKeys.Remove(entityReference);
            RemoveForeignKeyFromCacheInternal(fkType, entityReference);
        }

        internal void UpdateReferencedComponent<TComponent>(in EntityReference other)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            var fkType = ForeignKeyTag<TComponent>.wrapper;

            var fkCache = GetOrAddForeignKeyCache(fkType);
            var componentCache = GetOrAddComponentCache<ExclusiveGroupStruct>(fkType);

            if (fkCache.reverseForiegnKey.TryGetValue(other, out var otherHashSet))
            {
                if (!TryGetEGID(other, out var otherID) ||
                    FindTable<IReferenceableRow<TComponent>>(otherID.groupID) == null)
                {
                    foreach (var entityReference in otherHashSet)
                    {
                        var egid = entitiesDB.GetEGID(entityReference);
                        RemoveFromFilterInternal(fkType, componentCache, egid, entityReference);
                    }
                }
                else
                {
                    foreach (var entityReference in otherHashSet)
                    {
                        var egid = entitiesDB.GetEGID(entityReference);
                        UpdateFilterInternal(fkType, componentCache, egid, entityReference, otherID.groupID);
                    }
                }
            }
        }

        internal void RemoveReferencedComponent<TComponent>(in EntityReference other)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            var fkType = ForeignKeyTag<TComponent>.wrapper;

            var fkCache = GetOrAddForeignKeyCache(fkType);
            var componentCache = GetOrAddComponentCache<ExclusiveGroupStruct>(fkType);

            if (fkCache.reverseForiegnKey.TryGetValue(other, out var otherHashSet))
            {
                foreach (var entityReference in otherHashSet)
                {
                    var egid = entitiesDB.GetEGID(entityReference);
                    RemoveFromFilterInternal(fkType, componentCache, egid, entityReference);
                }

                fkCache.reverseForiegnKey.Remove(other);
            }
        }
    }
}
