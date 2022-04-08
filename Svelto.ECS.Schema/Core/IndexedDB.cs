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
    public sealed partial class IndexedDB : IStepEngine
    {
        internal readonly FasterList<SchemaMetadata> registeredSchemas = new();
        internal readonly FasterList<IEntityStateMachine> registeredStateMachines = new();

        // engines will be created per TComponent
        internal readonly HashSet<RefWrapperType> createdIndexerEngines = new();
        internal readonly HashSet<RefWrapperType> createdStateMachineEngines = new();

        internal readonly FasterDictionary<uint, IndexerData> indexers = new();

        internal IEntityFunctions entityFunctions;
        internal EntitiesDB entitiesDB;

        private readonly IStepEngine _engine;
        private readonly FasterList<IStepEngine> enginesList = new();

        internal IndexedDB(EnginesRoot enginesRoot, IEntityFunctions entityFunctions)
        {
            this.entityFunctions = entityFunctions;

            var pkEngine = new PrimaryKeyEngine(this);
            enginesRoot.AddEngine(pkEngine);
            enginesList.Add(pkEngine);

            _engine = new IndexedDBEngine(this, enginesList);
            enginesRoot.AddEngine(_engine);
        }

        internal void RegisterSchema(EnginesRoot enginesRoot, SchemaMetadata metadata)
        {
            registeredSchemas.Add(metadata);

            _groupToTable.Union(metadata.groupToTable);

            foreach (var indexer in metadata.indexers)
            {
                var componentType = indexer.ComponentType;

                if (createdIndexerEngines.Contains(componentType))
                    continue;

                createdIndexerEngines.Add(componentType);

                indexer.AddEngines(enginesRoot, this);
            }
        }

        internal void RegisterStateMachine<T>(EnginesRoot enginesRoot, T stateMachine)
            where T : class, IEntityStateMachine
        {
            var indexer = stateMachine.Index;

            if (!createdIndexerEngines.Contains(indexer.ComponentType))
            {
                createdIndexerEngines.Add(indexer.ComponentType);

                indexer.AddEngines(enginesRoot, this);
            }

            var componentType = stateMachine.ComponentType;

            if (!createdStateMachineEngines.Contains(componentType))
            {
                createdStateMachineEngines.Add(componentType);

                stateMachine.AddEngines(enginesRoot, this);
            }

            registeredStateMachines.Add(stateMachine);
        }

        public static implicit operator EntitiesDB(IndexedDB indexedDB) => indexedDB.entitiesDB;

        string IStepEngine.name => _engine.name;

        public void Step() => _engine.Step();
    }
}
