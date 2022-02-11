namespace Svelto.ECS.Schema.Definition
{
    public sealed class Partition<T> : EntitySchemaElement where T : IEntityShard, new()
    {
        public Partition(int range = 1)
        {
            this.range = range;
        }

        public Accessor At(ShardOffset offset) => new Accessor(this, offset);

        public T Shard(int index = 0)
        {
            return GetShard(metadata.root, index);
        }

        private T GetShard(SchemaMetadata.PartitionNode parent, int index)
        {
            var node = parent.partitions[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node");

            return new T { Offset = new ShardOffset { node = node, index = index } };
        }

        public struct Accessor
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
        }
    }
}