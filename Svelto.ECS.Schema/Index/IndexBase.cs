using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalIndexCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public interface IIndexedComponent<T> : IEntityComponent, INeedEGID
        where T : unmanaged
    {
        T Value { get; }
    }

    public abstract class IndexBase
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        protected internal readonly int _indexerId = GlobalIndexCount.Generate();

        internal IndexBase() { }
    }

    public abstract class IndexBase<TK, TC> : IndexBase, ISchemaDefinitionIndex
        where TK : unmanaged
        where TC : unmanaged, IIndexedComponent<TK>
    {
        RefWrapperType ISchemaDefinitionIndex.ComponentType => TypeRefWrapper<TC>.wrapper;

        int ISchemaDefinitionIndex.IndexerID => _indexerId;

        void ISchemaDefinitionIndex.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new TableIndexingEngine<TK, TC>(indexedDB));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexQuery<TK, TC> Query(in TK key)
        {
            return new IndexQuery<TK, TC>(_indexerId, key);
        }
    }
}
