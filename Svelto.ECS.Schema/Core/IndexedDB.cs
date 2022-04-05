using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// *Indexed* EntitiesDB
    /// </summary>
    public sealed partial class IndexedDB
    {
        internal readonly FasterList<SchemaMetadata> registeredSchemas = new();

        // indexer will be created per TComponent
        internal readonly HashSet<RefWrapperType> createdIndexerEngines = new();
        internal readonly HashSet<RefWrapperType> createdStateMachineEngines = new();

        internal readonly FasterDictionary<uint, IndexerData> indexers = new();

        internal IEntityFunctions entityFunctions;
        internal EntitiesDB entitiesDB;

        public IStepEngine Engine { get; private set; }
        private readonly FasterList<IStepEngine> enginesList = new();

        internal IndexedDB(IEntityFunctions entityFunctions)
        {
            this.entityFunctions = entityFunctions;

            Engine = new IndexedDBEngine(this, enginesList);
        }

        internal void RegisterSchema(EnginesRoot enginesRoot, SchemaMetadata metadata)
        {
            registeredSchemas.Add(metadata);

            var indexers = metadata.indexersToGenerateEngine.GetValues(out var count);

            for (int i = 0; i < count; ++i)
            {
                var componentType = metadata.indexersToGenerateEngine.unsafeKeys[i].key;

                if (createdIndexerEngines.Contains(componentType))
                    continue;

                createdIndexerEngines.Add(componentType);
                indexers[i].AddEngines(enginesRoot, this);
            }

            var stateMachines = metadata.stateMachinesToGenerateEngine.GetValues(out count);

            for (int i = 0; i < count; ++i)
            {
                var componentType = metadata.stateMachinesToGenerateEngine.unsafeKeys[i].key;

                if (createdStateMachineEngines.Contains(componentType))
                    continue;

                createdStateMachineEngines.Add(componentType);
                var stateMachineEngine = stateMachines[i].AddEngines(enginesRoot, this);

                enginesList.Add(stateMachineEngine);
            }

            var pkEngine = new PrimaryKeyEngine(this);

            enginesRoot.AddEngine(pkEngine);
            enginesList.Add(pkEngine);
        }

        public static implicit operator EntitiesDB(IndexedDB indexedDB) => indexedDB.entitiesDB;
    }
}
