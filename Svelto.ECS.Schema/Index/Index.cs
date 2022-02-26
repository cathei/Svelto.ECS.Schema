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
            protected internal readonly int _indexerId = GlobalIndexCount.Generate();

            internal IndexBase() { }
        }

        public abstract class IndexBase<TK, TC> : IndexBase<TK>, ISchemaDefinitionIndex
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            RefWrapperType ISchemaDefinitionIndex.KeyType => TypeRefWrapper<TK>.wrapper;

            int ISchemaDefinitionIndex.IndexerID => _indexerId;

            void ISchemaDefinitionIndex.AddEngines(IndexesDB indexesDB)
            {
                indexesDB.enginesRoot.AddEngine(new TableIndexingEngine<TK, TC>(indexesDB));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IndexQuery<TK, TC> Query(in TK key)
            {
                return new IndexQuery<TK, TC>(_indexerId, key);
            }
        }
    }

    namespace Definition
    {
        public sealed class Index<T> : IndexBase<T, Indexed<T>>
            where T : unmanaged, IEntityIndexKey<T>
        {

        }
    }
}
