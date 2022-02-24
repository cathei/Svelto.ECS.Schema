using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Definition
{
    internal static class GlobalMemoCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public sealed class Memo<T> : IEntitySchemaMemo
        where T : unmanaged, IEntityComponent
    {
        public void Add(IndexesDB indexesDB, EGID egid)
        {

        }
    }
}