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

        public class IndexBase<T> : IEntitySchemaIndex
            where T : unmanaged, IEntityIndexKey<T>
        {
            // equvalent to ExclusiveGroupStruct.Generate()
            private readonly int _indexerId = GlobalIndexCount.Generate();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IndexQuery<T> Query(in T key)
            {
                return new IndexQuery<T>(_indexerId, key);
            }

            IEngine IEntitySchemaIndex.CreateEngine(IndexesDB context)
            {
                return new TableIndexingEngine<T>(context);
            }

            RefWrapperType IEntitySchemaIndex.KeyType => TypeRefWrapper<T>.wrapper;

            int IEntitySchemaIndex.IndexerID => _indexerId;
        }
    }

    namespace Definition
    {
        public sealed class Index<T> : IndexBase<T>
            where T : unmanaged, IEntityIndexKey<T>
        {

        }

        public sealed class Index<T1, T2> : IndexBase<MultiIndexKey<T1, T2>>
            where T1 : unmanaged, IEntityIndexKey<T1>
            where T2 : unmanaged, IEntityIndexKey<T2>
        {

        }
    }

}