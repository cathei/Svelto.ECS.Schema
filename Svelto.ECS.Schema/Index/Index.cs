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

    public class IndexBase<TRow, TKey, TComponent> : IEntityIndex, IIndexQueryable<TRow, TKey>
        where TRow : class, IReactiveRow<TComponent>
        where TKey : unmanaged
        where TComponent : unmanaged, IIndexableComponent<TKey>
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        private readonly int _indexerId = GlobalIndexCount.Generate();

        RefWrapperType IEntityIndex.ComponentType => TypeRefWrapper<TComponent>.wrapper;

        int IEntityIndex.IndexerID => _indexerId;

        internal IndexBase() { }

        void IEntityIndex.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new TableIndexingEngine<TRow, TKey, TComponent>(indexedDB));
        }

        public IndexQuery<TRow, TKey> Query(in TKey key)
        {
            return new IndexQuery<TRow, TKey>(_indexerId, key);
        }
    }
}

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Index<TKey> : IndexBase<IIndexedRow<TKey>, TKey, Indexed<TKey>>
        where TKey : unmanaged, IIndexKey<TKey>
    { }

}
