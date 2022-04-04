using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public abstract class IndexBase<TRow, TComponent> : IEntityIndex, IIndexQueryable<TRow, TComponent>
        where TRow : class, IQueryableRow<ResultSet<TComponent>>
        where TComponent : unmanaged, IKeyComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly FilterContextID _indexerID = EntitiesDB.SveltoFilters.GetNewContextID();

        RefWrapperType IEntityIndex.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        FilterContextID IEntityIndex.IndexerID => _indexerID;
        FilterContextID IIndexQueryable<TRow, TComponent>.IndexerID => _indexerID;

        static IndexBase()
        {
            // must register and trigger reflection
            default(TComponent).Warmup<TComponent>();
        }

        internal IndexBase() { }

        void IEntityIndex.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            IndexableComponentHelper<TComponent>.EngineHandler.AddEngines<TRow>(enginesRoot, indexedDB);
        }
    }
}

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Index<TComponent> : IndexBase<IIndexableRow<TComponent>, TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }
}
