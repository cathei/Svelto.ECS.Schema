using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// *Indexed* EntitiesDB
    /// </summary>
    public sealed partial class IndexedDB
    {
        internal readonly FasterList<SchemaMetadata> registeredSchemas = new FasterList<SchemaMetadata>();
        internal readonly FasterList<IEntityStateMachine> registeredStateMachines = new FasterList<IEntityStateMachine>();

        // indexer will be created per TComponent
        internal readonly HashSet<RefWrapperType> createdIndexerEngines = new HashSet<RefWrapperType>();

        internal readonly FasterDictionary<int, IndexerData> indexers = new FasterDictionary<int, IndexerData>();
        internal readonly FasterDictionary<int, MemoData> memos = new FasterDictionary<int, MemoData>();

        // well... let's have some space for user defined filter
        private int filterIdCounter = 10000;

        internal EntitiesDB entitiesDB;

        internal void RegisterSchema(EnginesRoot enginesRoot, SchemaMetadata metadata)
        {
            registeredSchemas.Add(metadata);

            var indexers = metadata.indexersToGenerateEngine;

            foreach (var componentType in indexers.keys)
            {
                if (createdIndexerEngines.Contains(componentType))
                    continue;

                createdIndexerEngines.Add(componentType);
                indexers[componentType].AddEngines(enginesRoot, this);
            }
        }

        internal void RegisterStateMachine<T>(EnginesRoot enginesRoot, T stateMachine)
            where T : class, IEntityStateMachine
        {
            registeredStateMachines.Add(stateMachine);

            var componentType = stateMachine.Index.ComponentType;

            // we do NOT support multiple instance of same state machine
            if (!createdIndexerEngines.Contains(componentType))
            {
                createdIndexerEngines.Add(componentType);
                stateMachine.AddEngines(enginesRoot, this);
            }
        }

        internal void NotifyKeyUpdate<TK, TC>(ref TC keyComponent, in TK oldKey, in TK newKey)
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : struct, IIndexableComponent<TK>
        {
            // component updated but key didn't change
            if (oldKey.Equals(newKey))
                return;

            var indexers = FindIndexers<TK, TC>(keyComponent.ID.groupID);

            foreach (var indexer in indexers)
                UpdateFilters(indexer.IndexerID, ref keyComponent, oldKey, newKey);
        }

        public static implicit operator EntitiesDB(IndexedDB indexedDB) => indexedDB.entitiesDB;
    }
}
