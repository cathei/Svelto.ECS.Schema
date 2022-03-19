using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public interface ISchemaDefinition { }

        public interface IEntityTable : ISchemaDefinition
        {
            internal EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
            internal void Remove(IEntityFunctions functions, uint entityID, in ExclusiveGroupStruct groupID);
        }

        public interface IEntityIndex : ISchemaDefinition
        {
            RefWrapperType ComponentType { get; }
            int IndexerID { get; }
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        }

        internal interface ISchemaDefinitionRangedSchema : ISchemaDefinition
        {
            int Range { get; }
            IEntitySchema GetSchema(int index);
        }

        internal interface ISchemaDefinitionMemo : ISchemaDefinition
        {
            int MemoID { get; }
        }

        public interface IEntityTables
        {
            int Range { get; }
            IEntityTable GetTable(int index);
        }

        public interface IEntityStateMachine
        {
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
            IEntityIndex Index { get; }
        }
    }

    namespace Definition
    {
        public interface IEntitySchema : ISchemaDefinition { }
    }
}
