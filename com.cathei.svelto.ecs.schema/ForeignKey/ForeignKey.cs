using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public struct ForeignKeyTag<TComponent>
    {
        public static RefWrapperType wrapper => TypeRefWrapper<ForeignKeyTag<TComponent>>.wrapper;
    }
}

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// Foreign key is used to mimic Join operation
    /// Foreign key backend is special filter to map groups so join work
    /// </summary>
    public sealed class ForeignKey<TComponent, TReferRow> :
            IForeignKeyDefinition, IForeignKey<TComponent, TReferRow>
        where TComponent : unmanaged, IForeignKeyComponent
        where TReferRow : class, IReferenceableRow<TComponent>
    {
        internal sealed class Index : IIndexDefinition
        {
            // equvalent to ExclusiveGroupStruct.Generate()
            internal readonly FilterContextID _indexerID = EntitiesDB.SveltoFilters.GetNewContextID();

            public FilterContextID IndexerID => _indexerID;

            // use foreign key tag to avoid conflict with regular index
            public RefWrapperType ComponentType => ForeignKeyTag<TComponent>.wrapper;

            public void AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
            {
                enginesRoot.AddEngine(new ForeignKeyEngine<TComponent>(indexedDB));
            }
        }

        internal Index _index = new();

        IIndexDefinition IForeignKeyDefinition.Index => _index;

        public FilterContextID IndexerID => _index._indexerID;

        bool IJoinProvider.IsValidGroup(IndexedDB indexedDB, ExclusiveGroupStruct group)
        {
            return indexedDB.FindTable<TReferRow>(group) != null;
        }
    }
}
