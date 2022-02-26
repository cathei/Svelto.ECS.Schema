using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public interface ISchemaDefinition { }

        internal interface ISchemaDefinitionTable : ISchemaDefinition
        {
            ref readonly ExclusiveGroupStruct ExclusiveGroup { get; }
            void Remove(IEntityFunctions functions, uint entityID);
        }

        internal interface ISchemaDefinitionIndex : ISchemaDefinition
        {
            RefWrapperType KeyType { get; }
            int IndexerID { get; }
            void AddEngines(IndexesDB indexesDB);
        }

        internal interface ISchemaDefinitionRangedTable : ISchemaDefinition
        {
            int Range { get; }
            ISchemaDefinitionTable GetTable(int index);
        }

        internal interface ISchemaDefinitionRangedSchema : ISchemaDefinition
        {
            Type InnerType { get; }
            int Range { get; }
            IEntitySchema GetSchema(int index);
        }

        internal interface ISchemaDefinitionMemo : ISchemaDefinition
        {
            int MemoID { get; }
        }
    }

    public interface IEntitySchema : Internal.ISchemaDefinition { }

    public interface IEntityStateMachine
    {
        IStepEngine AddEngines(IndexesDB indexesDB);
    }
}
