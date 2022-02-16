using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly struct ShardEnumerable<T> where T : class, IEntityShard
    {
        internal readonly IEnumerable<T> shards;

        public delegate GroupsBuilder<TDesc> GroupsBuilderSelector<TDesc>(T shard) where TDesc : IEntityDescriptor, new();

        internal ShardEnumerable(IEnumerable<T> shards)
        {
            this.shards = shards;
        }

        public ShardEnumerable<TOut> Combine<TOut>(Func<T, TOut> selector) where TOut : class, IEntityShard
        {
            return new ShardEnumerable<TOut>(shards.Select(selector));
        }

        public ShardEnumerable<TOut> Combine<TOut>(Func<T, ShardEnumerable<TOut>> selector) where TOut : class, IEntityShard
        {
            return new ShardEnumerable<TOut>(Unpack(selector));
        }

        public GroupsBuilder<TDesc> Combine<TDesc>(Func<T, Group<TDesc>> selector) where TDesc : IEntityDescriptor, new()
        {
            return new GroupsBuilder<TDesc>(shards.Select(x => selector(x).exclusiveGroup));
        }

        public GroupsBuilder<TDesc> Combine<TDesc>(GroupsBuilderSelector<TDesc> selector) where TDesc : IEntityDescriptor, new()
        {
            return new GroupsBuilder<TDesc>(Unpack(selector));
        }

        // just SelectMany...
        internal IEnumerable<TOut> Unpack<TOut>(Func<T, ShardEnumerable<TOut>> selector) where TOut : class, IEntityShard
        {
            foreach (var shard in shards)
            {
                foreach (var shardOut in selector(shard).shards)
                    yield return shardOut;
            }
        }

        internal IEnumerable<ExclusiveGroupStruct> Unpack<TDesc>(GroupsBuilderSelector<TDesc> selector) where TDesc : IEntityDescriptor, new()
        {
            foreach (var shard in shards)
            {
                foreach (var group in selector(shard).items)
                    yield return group;
            }
        }
    }
}