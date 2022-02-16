using System;
using System.Collections.Generic;
using System.Linq;

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

        public T Shard(int index = 0) => GetShard(index);

        public ShardEnumerable<T> Shards() => new ShardEnumerable<T>(GetShards(Enumerable.Range(0, range)));
        public ShardEnumerable<T> Shards(IEnumerable<int> indexes) => new ShardEnumerable<T>(GetShards(indexes));

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