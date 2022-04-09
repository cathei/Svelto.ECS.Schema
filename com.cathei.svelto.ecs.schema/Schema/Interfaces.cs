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

            internal FasterDictionary<int, IEntityPrimaryKey> PrimaryKeys { get; }

            internal EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
            internal void Swap(IEntityFunctions functions, in EGID egid, in ExclusiveBuildGroup groupID);
            internal void Remove(IEntityFunctions functions, in EGID egid);
        }

        public interface IEntityIndex : ISchemaDefinition
        {
            RefWrapperType ComponentType { get; }
            FilterContextID IndexerID { get; }

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

        internal interface IEntityMemo : ISchemaDefinition
        {
            CombinedFilterID FilterID { get; }
        }

        public interface IEntityForeignKey : ISchemaDefinition
        {
            IEntityIndex Index { get; }
        }

        public interface IEntityStateMachine
        {
            RefWrapperType ComponentType { get; }
            void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
            IEntityIndex Index { get; }
        }

        public interface IEntityTables
        {
            int Range { get; }
            IEntityTable GetTable(int index);
        }
    }

    public interface IEntitySchema : ISchemaDefinition { }
}
