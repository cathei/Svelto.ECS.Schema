using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly struct ShardEnumerable<T> where T : struct, IEntityShard
    {
        internal readonly IEnumerable<T> shards;

        internal ShardEnumerable(SchemaMetadata.PartitionNode node, IEnumerable<int> indexes)
            : this(indexes.Select(i => new T { Offset = new ShardOffset(node, i) })) { }

        internal ShardEnumerable(IEnumerable<T> shards)
        {
            this.shards = shards;
        }

        public ShardEnumerable<TOut> Each<TOut>(Func<T, TOut> selector) where TOut : struct, IEntityShard
        {
            return new ShardEnumerable<TOut>(shards.Select(selector));
        }

        public ShardEnumerable<TOut> Each<TOut>(Func<T, ShardEnumerable<TOut>> selector) where TOut : struct, IEntityShard
        {
            return new ShardEnumerable<TOut>(Unpack(selector));
        }

        public Groups<TDesc> Each<TDesc>(Func<T, Group<TDesc>> selector) where TDesc : IEntityDescriptor
        {
            return new Groups<TDesc>(shards.Select(x => selector(x).exclusiveGroup));
        }

        public Groups<TDesc> Each<TDesc>(Func<T, Groups<TDesc>> selector) where TDesc : IEntityDescriptor
        {
            return new Groups<TDesc>(Unpack(selector));
        }

        // just SelectMany...
        internal IEnumerable<TOut> Unpack<TOut>(Func<T, ShardEnumerable<TOut>> selector) where TOut : struct, IEntityShard
        {
            foreach (var shard in shards)
            {
                foreach (var shardOut in selector(shard).shards)
                    yield return shardOut;
            }
        }

        internal IEnumerable<ExclusiveGroupStruct> Unpack<TDesc>(Func<T, Groups<TDesc>> selector) where TDesc : IEntityDescriptor
        {
            foreach (var shard in shards)
            {
                foreach (var group in selector(shard).items)
                    yield return group;
            }
        }
    }
}