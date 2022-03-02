using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema.Internal
{
    internal static class GlobalIndexCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public interface IIndexableComponent<T> : IEntityComponent, INeedEGID
        where T : unmanaged
    {
        T Value { get; }
    }

    public interface IIndexableRow<TK, TC> : IEntityRow<TC>
        where TK : unmanaged
        where TC : unmanaged, IIndexableComponent<TK>
    { }

    public abstract class IndexBase<TR, TK, TC> : ISchemaDefinitionIndex, IIndexQueryable<TR, TK, TC>
        where TR : IIndexableRow<TK, TC>
        where TK : unmanaged
        where TC : unmanaged, IIndexableComponent<TK>
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        protected internal readonly int _indexerId = GlobalIndexCount.Generate();

        RefWrapperType ISchemaDefinitionIndex.ComponentType => TypeRefWrapper<TC>.wrapper;

        int ISchemaDefinitionIndex.IndexerID => _indexerId;

        internal IndexBase() { }

        void ISchemaDefinitionIndex.AddEngines(EnginesRoot enginesRoot, IndexedDB indexedDB)
        {
            enginesRoot.AddEngine(new TableIndexingEngine<TK, TC>(indexedDB));
        }

        public IndexQuery<TR, TK, TC> Query(in TK key)
        {
            return new IndexQuery<TR, TK, TC>(_indexerId, key);
        }
    }
}
