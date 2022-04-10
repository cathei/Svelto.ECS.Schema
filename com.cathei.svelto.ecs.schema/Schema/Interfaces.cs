using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface ISchemaElementDefinition { }

    public interface ITableDefinition : ISchemaElementDefinition
    {
        ref readonly ExclusiveGroup Group { get; }
        int GroupRange { get; }

        internal FasterDictionary<int, IPrimaryKeyDefinition> PrimaryKeys { get; }

        internal EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
        internal void Swap(IEntityFunctions functions, in EGID egid, in ExclusiveBuildGroup groupID);
        internal void Remove(IEntityFunctions functions, in EGID egid);
    }

    public interface IIndexDefinition : ISchemaElementDefinition
    {
        RefWrapperType ComponentType { get; }
        FilterContextID IndexerID { get; }

        void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
    }

    public interface IPrimaryKeyDefinition : ISchemaElementDefinition
    {
        int PrimaryKeyID { get; }
        Delegate KeyToIndex { get; }
        ushort PossibleKeyCount { get; }

        internal void Ready(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);
        internal int QueryGroupIndex(uint index);
    }

    internal interface IMemoDefinition : ISchemaElementDefinition
    {
        CombinedFilterID FilterID { get; }
    }

    public interface IForeignKeyDefinition : ISchemaElementDefinition
    {
        IIndexDefinition Index { get; }
    }

    public interface IEntityStateMachine
    {
        RefWrapperType ComponentType { get; }
        void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB);
        IIndexDefinition Index { get; }
    }

    public interface ITablesDefinition
    {
        int Range { get; }
        ITableDefinition GetTable(int index);
    }
}

namespace Svelto.ECS.Schema
{
    public interface IEntitySchema : ISchemaElementDefinition { }
}
