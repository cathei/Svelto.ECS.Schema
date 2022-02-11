namespace Svelto.ECS.Schema.Definition
{
    public sealed class Table<T> : EntitySchemaElement where T : IEntityDescriptor
    {
        public Table(int range = 1)
        {
            this.range = range;
        }

        public Accessor At(ShardOffset offset) => new Accessor(this, offset);

        public ExclusiveGroupStruct Group(int index = 0)
        {
            return GetGroup(metadata.root, index);
        }

        private ExclusiveGroupStruct GetGroup(SchemaMetadata.PartitionNode parent, int index)
        {
            var node = parent.tables[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node");

            return node.group + (ushort)index;
        }

        public struct Accessor
        {
            private readonly Table<T> table;
            private readonly ShardOffset offset;

            internal Accessor(Table<T> table, ShardOffset offset)
            {
                this.table = table;
                this.offset = offset;
            }

            public ExclusiveGroupStruct Group(int index = 0)
            {
                return table.GetGroup(offset.node, (offset.index * table.range) + index);
            }
        }
    }
}