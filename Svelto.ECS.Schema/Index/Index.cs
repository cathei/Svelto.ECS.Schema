using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        internal static class GlobalIndexCount
        {
            private static int Count = 0;

            public static int Generate() => Interlocked.Increment(ref Count);
        }

        public abstract class IndexBase<TK>
            where TK : unmanaged, IKeyEquatable<TK>
        {
            // equvalent to ExclusiveGroupStruct.Generate()
            protected readonly int _indexerId = GlobalIndexCount.Generate();

            internal IndexBase() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IndexQuery<TK> Query(in TK key)
            {
                return new IndexQuery<TK>(_indexerId, key);
            }
        }

        public abstract class IndexBase<TK, TC> : IndexBase<TK>, IEntitySchemaIndex
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            RefWrapperType IEntitySchemaIndex.KeyType => TypeRefWrapper<TK>.wrapper;

            int IEntitySchemaIndex.IndexerID => _indexerId;

            void IEntitySchemaIndex.AddEngines(EnginesRoot enginesRoot, IndexesDB indexesDB)
            {
                enginesRoot.AddEngine(new TableIndexingEngine<TK, TC>(indexesDB));
            }
        }
    }

    namespace Definition
    {
        public sealed class Index<T> : IndexBase<T, IEntityIndexKey<T>.Component>
            where T : unmanaged, IEntityIndexKey<T>
        {

        }
    }
}
