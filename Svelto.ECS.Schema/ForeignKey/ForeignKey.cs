using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// Foreign key is used to mimic Join operation
    /// Foreign key backend is special filter to map groups so join work
    /// </summary>
    public sealed class ForeignKey<TComponent> : IEntityForeignKey
        where TComponent : unmanaged, IForeignKeyComponent
    {
        internal sealed class Index : IEntityIndex
        {
            // equvalent to ExclusiveGroupStruct.Generate()
            internal readonly FilterContextID _indexerID = EntitiesDB.SveltoFilters.GetNewContextID();

            public FilterContextID IndexerID => _indexerID;

            // use foreign key type to avoid conflict with regular index
            public RefWrapperType ComponentType => TypeRefWrapper<ForeignKey<TComponent>>.wrapper;

            public void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB) { }
        }

        internal Index _index = new();

        public RefWrapperType ComponentType => TypeRefWrapper<TComponent>.wrapper;

        IEntityIndex IEntityForeignKey.Index => _index;

        public void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new ForeignKeyEngine<TComponent>(indexedDB));
        }
    }
}
