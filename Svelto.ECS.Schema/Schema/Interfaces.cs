using System;
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
            string Name { get; set; }

            ref readonly ExclusiveGroupStruct ExclusiveGroup { get; }
        }

        public interface IEntityIndex : ISchemaDefinition
        {
            RefWrapperType ComponentType { get; }
            int IndexerID { get; }
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        }

        public interface IEntityTables : ISchemaDefinition
        {
            string Name { get; set; }

            bool IsCombined { get; }
            int Range { get; }
            LocalFasterReadOnlyList<ExclusiveGroupStruct> ExclusiveGroups { get; }
            IEntityTable GetTable(int index);
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
            IEntityIndex Index { get; }
        }
    }

    namespace Definition
    {
        public interface IEntitySchema : ISchemaDefinition { }
    }
}
