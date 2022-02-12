using System.Collections.Generic;
using System.Linq;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Partition<T> : EntitySchemaElement where T : struct, IEntityShard
    {
        public Partition(int range = 1)
        {
            this.range = range;
        }

        public Accessor At(ShardOffset offset) => new Accessor(this, offset);

        public T Shard(int index = 0) => GetShard(metadata.root, index);

        public ShardEnumerable<T> Shards() => GetShards(metadata.root, Enumerable.Range(0, range));

        public ShardEnumerable<T> Shards(IEnumerable<int> indexes) => GetShards(metadata.root, indexes);

        private T GetShard(SchemaMetadata.PartitionNode parent, int index)
        {
            var node = parent.partitions[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node. Did you forget to call .At(Offset)?");

            return new T { Offset = new ShardOffset(node, index) };
        }

        private ShardEnumerable<T> GetShards(SchemaMetadata.PartitionNode parent, IEnumerable<int> indexes)
        {
            var node = parent.partitions[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node. Did you forget to call .At(Offset)?");

            return new ShardEnumerable<T>(node, indexes);
        }

        public readonly struct Accessor
        {
            private readonly Partition<T> partition;
            private readonly ShardOffset offset;

            internal Accessor(Partition<T> partition, ShardOffset offset)
            {
                this.partition = partition;
                this.offset = offset;
            }

            public T Shard(int index = 0)
            {
                return partition.GetShard(offset.node, (offset.index * partition.range) + index);
            }

            public ShardEnumerable<T> Shards()
            {
                int start = offset.index * partition.range;
                return partition.GetShards(offset.node, Enumerable.Range(start, partition.range));
            }

            public ShardEnumerable<T> Shards(IEnumerable<int> indexes)
            {
                int start = offset.index * partition.range;
                return partition.GetShards(offset.node, indexes.Select(i => start + i));
            }
        }
    }
}