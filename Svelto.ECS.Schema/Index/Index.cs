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
}

namespace Svelto.ECS.Schema.Definition
{
    public class Index<TData, TKey> : IEntityIndex, IIndexQueryable<IIndexedRow<TData>, TKey>
        where TData : unmanaged, IIndexedData<TKey>
        where TKey : unmanaged
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        protected internal readonly int _indexerId = GlobalIndexCount.Generate();

        RefWrapperType IEntityIndex.ComponentType => TypeRefWrapper<Indexed<TData>>.wrapper;

        int IEntityIndex.IndexerID => _indexerId;

        internal Index() { }

        void IEntityIndex.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new TableIndexingEngine<IIndexedRow<TData>, TKey, Indexed<TData>>(
                indexedDB, component => component._data.Key));
        }

        public IndexQuery<IIndexedRow<TData>, TKey> Query(in TKey key)
        {
            return new IndexQuery<IIndexedRow<TData>, TKey>(_indexerId, key);
        }
    }
}
