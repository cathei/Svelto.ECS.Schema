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
            public FasterDictionary<EntityReference, FasterDictionary<EntityReference, EntityReference>> reverseForiegnKey = new();
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

                if (fkCache.reverseForiegnKey.TryGetValue(other, out var reverseReferences))
                    reverseReferences.Remove(entityReference);
            }
        }

        internal void UpdateForeignKeyComponent<TComponent>(in EGID egid, in EntityReference other)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            UpdateForeignKeyComponent<TComponent>(egid, GetEntityReference(egid), other);
        }

        internal void UpdateForeignKeyComponent<TComponent>(
                in EGID egid, in EntityReference entityReference, in EntityReference other)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            var fkType = ForeignKeyTag<TComponent>.wrapper;

            var componentCache = GetOrAddComponentCache<ExclusiveGroupStruct>(fkType);
            var fkCache = GetOrAddForeignKeyCache(fkType);

            // we need to compare with previous key with reference because it's only reliable value
            if (fkCache.previousForeignKey.TryGetValue(entityReference, out var previousReference) &&
                fkCache.reverseForiegnKey.TryGetValue(previousReference, out var previousReverseReferences))
            {
                previousReverseReferences.Remove(entityReference);
            }

            fkCache.previousForeignKey[entityReference] = other;

            var reverseReferences = fkCache.reverseForiegnKey.GetOrAdd(other, () => new());
            reverseReferences.Add(entityReference, entityReference);

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

        internal void RemoveForeignKeyComponent<TComponent>(in EntityReference entityReference)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            var fkType = ForeignKeyTag<TComponent>.wrapper;
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

            if (fkCache.reverseForiegnKey.TryGetValue(other, out var reverseReferences))
            {
                var reverseReferencesValues = reverseReferences.GetValues(out var count);

                if (!TryGetEGID(other, out var otherID) ||
                    FindTable<IReferenceableRow<TComponent>>(otherID.groupID) == null)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        var egid = entitiesDB.GetEGID(reverseReferencesValues[i]);
                        RemoveFromFilterInternal(fkType, componentCache, egid, reverseReferencesValues[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        var egid = entitiesDB.GetEGID(reverseReferencesValues[i]);
                        UpdateFilterInternal(fkType, componentCache, egid, reverseReferencesValues[i], otherID.groupID);
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

            if (fkCache.reverseForiegnKey.TryGetValue(other, out var reverseReferences))
            {
                var reverseReferencesValues = reverseReferences.GetValues(out var count);

                for (int i = 0; i < count; ++i)
                {
                    var egid = entitiesDB.GetEGID(reverseReferencesValues[i]);
                    RemoveFromFilterInternal(fkType, componentCache, egid, reverseReferencesValues[i]);
                }

                fkCache.reverseForiegnKey.Remove(other);
            }
        }

        public ref struct ReverseReferences
        {
            private readonly EnginesRoot.LocatorMap locatorMap;
            private readonly MB<EntityReference> buffer;
            public readonly uint count;

            public ReverseReferences(
                in EnginesRoot.LocatorMap locatorMap, MB<EntityReference> buffer, uint count)
            {
                this.locatorMap = locatorMap;
                this.buffer = buffer;
                this.count = count;
            }

            public EntityReference this[int index] => buffer[index];
            public EntityReference this[uint index] => buffer[index];

            public Enumerator GetEnumerator() => new(locatorMap, buffer, count);

            public ref struct Enumerator
            {
                private readonly EnginesRoot.LocatorMap locatorMap;
                private readonly MB<EntityReference> buffer;
                private readonly uint count;
                private uint index;

                public Enumerator(
                    in EnginesRoot.LocatorMap locatorMap, MB<EntityReference> buffer, uint count)
                {
                    this.locatorMap = locatorMap;
                    this.buffer = buffer;
                    this.count = count;
                    index = 0;
                }

                public bool MoveNext() => ++index <= count;

                public void Reset() => index = 0;

                public void Dispose() { }

                public EGID Current => locatorMap.GetEGID(buffer[index - 1]);
            }
        }

        /// <summary>
        /// get reverese reference map, which is the list of referencers
        /// useful when you want to get all entities that reference this entity
        /// e.g. Remove all equipments from a specific character
        /// </summary>
        internal ReverseReferences QueryReverseReferences(
            IForeignKeyDefinition foreignKey, in EGID referencedEGID)
        {
            return QueryReverseReferences(foreignKey, GetEntityReference(referencedEGID));
        }

        /// <summary>
        /// get reverese reference map, which is the list of referencers
        /// useful when you want to get all entities that reference this entity
        /// e.g. Remove all equipments from a specific character
        /// </summary>
        internal ReverseReferences QueryReverseReferences(
                IForeignKeyDefinition foreignKey, in EntityReference referencedEntity)
        {
            var fkType = foreignKey.Index.ComponentType;
            var fkCache = GetOrAddForeignKeyCache(fkType);

            if (fkCache.reverseForiegnKey.TryGetValue(referencedEntity, out var reverseReferences))
            {
                var buffer = reverseReferences.GetValues(out var count);
                return new ReverseReferences(entitiesDB.GetEntityLocator(), buffer, count);
            }

            return default;
        }
    }
}
