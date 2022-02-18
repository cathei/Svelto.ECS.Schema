using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Partition<T> : IEntitySchemaPartition
        where T : class, IEntityShard, new()
    {
        internal readonly T[] shards;
        internal readonly int range;

        public int Range => range;

        public Type ShardType => typeof(T);

        public Partition(int range = 1)
        {
            this.range = range;

            shards = new T[range];
            for (int i = 0; i < range; ++i)
                shards[i] = new T();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Shard(int index = 0) => GetShard(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShardsBuilder<T> Shards() => new ShardsBuilder<T>(GetShards(Enumerable.Range(0, range)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShardsBuilder<T> Shards(IEnumerable<int> indexes) => new ShardsBuilder<T>(GetShards(indexes));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetShard(int index)
        {
            return shards[index];
        }

        private IEnumerable<T> GetShards(IEnumerable<int> indexes)
        {
            foreach (int i in indexes)
                yield return shards[i];
        }

        object IEntitySchemaPartition.GetShard(int index) => GetShard(index);
    }
}