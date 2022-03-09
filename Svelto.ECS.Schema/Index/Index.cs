using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalIndexCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public class IndexBase<TRow, TComponent> : IEntityIndex, IIndexQueryable<TRow, TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : unmanaged, IIndexableComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _indexerId = GlobalIndexCount.Generate();

        RefWrapperType IEntityIndex.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        int IEntityIndex.IndexerID => _indexerId;
        int IIndexQueryable<TRow, TComponent>.IndexerID => _indexerId;

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
        where TComponent : unmanaged, IIndexableComponent
    { }
}
