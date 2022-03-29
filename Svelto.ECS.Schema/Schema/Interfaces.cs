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
            ref readonly ExclusiveGroup Group { get; }
            int GroupRange { get; }

            internal EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
            internal void Swap(IEntityFunctions functions, in EGID egid, in ExclusiveBuildGroup groupID);
            internal void Remove(IEntityFunctions functions, uint entityID, in ExclusiveGroupStruct groupID);

            internal LocalFasterReadOnlyList<IEntityPrimaryKey> PrimaryKeys { get; }
        }

        public interface IEntityIndex : ISchemaDefinition
        {
            RefWrapperType ComponentType { get; }
            int IndexerID { get; }
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        }

        public interface IEntityPrimaryKey : ISchemaDefinition
        {
            int PrimaryKeyID { get; }
            Delegate KeyToIndex { get; }
            ushort PossibleKeyCount { get; }

            internal void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);
            internal int QueryGroupIndex(uint index);
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
            RefWrapperType ComponentType { get; }
            IStepEngine AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
            IEntityIndex Index { get; }
        }
    }

    namespace Definition
    {
        public interface IEntitySchema : ISchemaDefinition { }
    }
}
