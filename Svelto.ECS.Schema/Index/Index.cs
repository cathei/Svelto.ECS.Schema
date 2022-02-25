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

        public abstract class IndexBase<T>
            where T : unmanaged, IKeyEquatable<T>
        {
            // equvalent to ExclusiveGroupStruct.Generate()
            protected readonly int _indexerId = GlobalIndexCount.Generate();

            internal IndexBase() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IndexQuery<T> Query(in T key)
            {
                return new IndexQuery<T>(_indexerId, key);
            }
        }
    }

    namespace Definition
    {
        public sealed class Index<T> : IndexBase<T>, IEntitySchemaIndex
            where T : unmanaged, IEntityIndexKey<T>
        {
            RefWrapperType IEntitySchemaIndex.KeyType => TypeRefWrapper<T>.wrapper;

            int IEntitySchemaIndex.IndexerID => _indexerId;

            void IEntitySchemaIndex.AddEngines(EnginesRoot enginesRoot, IndexesDB indexesDB)
            {
                enginesRoot.AddEngine(new TableIndexingEngine<T>(indexesDB));
            }
        }
    }
}
