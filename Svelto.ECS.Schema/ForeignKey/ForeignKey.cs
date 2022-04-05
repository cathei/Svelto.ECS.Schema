using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// Foreign key is used to mimic Join operation
    /// Foreign key backend is special filter to map groups so join work
    /// </summary>
    public sealed class ForeignKey<TComponent>
        where TComponent : unmanaged, IForeignKeyComponent
    {
        // static ForeignKey()
        // {
        //     // ForeignKeyHelperImpl<TComponent>.Warmup();
        // }

        internal sealed class Index : IEntityIndex
        {
            // equvalent to ExclusiveGroupStruct.Generate()
            internal readonly FilterContextID _indexerID = EntitiesDB.SveltoFilters.GetNewContextID();

            public FilterContextID IndexerID => _indexerID;

            public RefWrapperType ComponentType => TypeRefWrapper<ForeignKey<TComponent>>.wrapper;

            public void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB) { }
        }

        internal Index _index = new();

        public void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new ForeignKeyEngine<TComponent>(indexedDB));
        }
    }
}
