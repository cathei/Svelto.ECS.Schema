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

        // engines will be created per TComponent
        internal readonly HashSet<RefWrapperType> createdIndexerEngines = new();
        internal readonly HashSet<RefWrapperType> createdForeignKeyEngines = new();
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

            _groupToTable.Union(metadata.groupToTable);

            foreach (var indexer in metadata.indexers)
            {
                var componentType = indexer.ComponentType;

                if (createdIndexerEngines.Contains(componentType))
                    continue;

                createdIndexerEngines.Add(componentType);

                indexer.AddEngines(enginesRoot, this);
            }

            foreach (var fk in metadata.foreignKeys)
            {
                var componentType = fk.ComponentType;

                if (createdForeignKeyEngines.Contains(componentType))
                    continue;

                createdForeignKeyEngines.Add(componentType);

                fk.AddEngines(enginesRoot, this);
            }

            foreach (var stateMachine in metadata.stateMachines)
            {
                var componentType = stateMachine.ComponentType;

                if (createdStateMachineEngines.Contains(componentType))
                    continue;

                createdStateMachineEngines.Add(componentType);

                var stateMachineEngine = stateMachine.AddEngines(enginesRoot, this);
                enginesList.Add(stateMachineEngine);
            }

            var pkEngine = new PrimaryKeyEngine(this);

            enginesRoot.AddEngine(pkEngine);
            enginesList.Add(pkEngine);
        }

        public static implicit operator EntitiesDB(IndexedDB indexedDB) => indexedDB.entitiesDB;
    }
}
