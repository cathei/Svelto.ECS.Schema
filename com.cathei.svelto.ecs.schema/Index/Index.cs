using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Index<TComponent> : IIndexDefinition, IEntityIndex<TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly FilterContextID _indexerID = EntitiesDB.SveltoFilters.GetNewContextID();

        RefWrapperType IIndexDefinition.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        FilterContextID IIndexDefinition.IndexerID => _indexerID;
        FilterContextID IIndexQueryable<IIndexableRow<TComponent>, TComponent>.IndexerID => _indexerID;

        static Index()
        {
            // must register and trigger reflection
            default(TComponent).Warmup<TComponent>();
        }

        internal Index() { }

        void IIndexDefinition.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new TableIndexingEngine<IIndexableRow<TComponent>, TComponent>(indexedDB));
        }
    }
}
