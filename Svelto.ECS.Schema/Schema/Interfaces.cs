using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public interface ISchemaDefinition { }

        public interface ISchemaDefinitionTable : ISchemaDefinition
        {
            ref readonly ExclusiveGroupStruct ExclusiveGroup { get; }
            void Remove(IEntityFunctions functions, uint entityID);
        }

        public interface ISchemaDefinitionIndex : ISchemaDefinition
        {
            RefWrapperType ComponentType { get; }
            int IndexerID { get; }
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        }

        internal interface ISchemaDefinitionTables : ISchemaDefinition
        {
            bool IsCombined { get; }
            int Range { get; }
            ISchemaDefinitionTable GetTable(int index);
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

        public interface IEntityStateMachine
        {
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
            ISchemaDefinitionIndex Index { get; }
        }
    }

    namespace Definition
    {
        public interface IEntitySchema : ISchemaDefinition { }
    }
}
