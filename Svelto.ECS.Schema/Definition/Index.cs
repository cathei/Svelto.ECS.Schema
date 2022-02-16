using System;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Definition
{
    internal static class GlobalIndexCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public sealed class Index<T> : IEntitySchemaIndex
        where T : unmanaged, IEntityIndexKey<T>
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal int indexerId = GlobalIndexCount.Generate();

        public IndexQuery<T> Query(in T key)
        {
            return new IndexQuery<T>(indexerId, key);
        }

        IEngine IEntitySchemaIndex.CreateEngine(SchemaContext context)
        {
            return new TableIndexingEngine<T>(context);
        }

        RefWrapperType IEntitySchemaIndex.KeyType => TypeRefWrapper<T>.wrapper;

        int IEntitySchemaIndex.IndexerId => indexerId;
    }
}