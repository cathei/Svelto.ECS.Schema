using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Definition
{
    internal static class GlobalSetCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public sealed class Set<T> : IEntitySchemaSet
        where T : unmanaged, IEntityComponent
    {
        public void Add(IndexesDB indexesDB, EGID egid)
        {
            // indexesDB.indexerSets.

        }
    }
}