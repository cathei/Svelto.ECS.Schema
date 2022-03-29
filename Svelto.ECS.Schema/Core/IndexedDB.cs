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
        internal readonly FasterList<SchemaMetadata> registeredSchemas = new FasterList<SchemaMetadata>();
        internal readonly FasterList<IEntityStateMachine> registeredStateMachines = new FasterList<IEntityStateMachine>();

        // indexer will be created per TComponent
        internal readonly HashSet<RefWrapperType> createdIndexerEngines = new HashSet<RefWrapperType>();
        internal readonly HashSet<RefWrapperType> createdStateMachineEngines = new HashSet<RefWrapperType>();

        internal readonly FasterDictionary<int, IndexerData> indexers = new FasterDictionary<int, IndexerData>();
        internal readonly FasterDictionary<int, MemoData> memos = new FasterDictionary<int, MemoData>();

        // well... let's have some space for user defined filter
        private int filterIdCounter = 10000;

        internal IEntityFunctions entityFunctions;
        internal EntitiesDB entitiesDB;

        public IStepEngine Engine { get; private set; }
        private readonly FasterList<IStepEngine> enginesList = new FasterList<IStepEngine>();

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

        public static implicit operator EntitiesDB(IndexedDB indexedDB) => indexedDB.entitiesDB;
    }
}
