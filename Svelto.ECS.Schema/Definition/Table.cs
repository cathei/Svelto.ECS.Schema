using System.Collections.Generic;
using System.Linq;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Table<T> : EntitySchemaElement where T : IEntityDescriptor
    {
        public Table(int range = 1)
        {
            this.range = range;
        }

        public Accessor At(ShardOffset offset) => new Accessor(this, offset);

        public Group<T> Group(int index = 0) => GetGroup(metadata.root, index);

        public Groups<T> Groups() => new Groups<T>(GetGroups(metadata.root, Enumerable.Range(0, range)));
        public Groups<T> Groups(IEnumerable<int> indexes) => new Groups<T>(GetGroups(metadata.root, indexes));

        private Group<T> GetGroup(SchemaMetadata.PartitionNode parent, int index)
        {
            var node = parent.tables[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node");

            return new Group<T>(node.group + (ushort)index);
        }

        private IEnumerable<ExclusiveGroupStruct> GetGroups(SchemaMetadata.PartitionNode parent, IEnumerable<int> indexes)
        {
            var node = parent.tables[siblingOrder];

            if (node.element != this)
                throw new ECSException("Cannot find correct node");

            foreach (int i in indexes)
                yield return node.group + (ushort)i;
        }

        public readonly struct Accessor
        {
            private readonly Table<T> table;
            private readonly ShardOffset offset;

            internal Accessor(Table<T> table, ShardOffset offset)
            {
                this.table = table;
                this.offset = offset;
            }

            public Group<T> Group(int index = 0) =>
                table.GetGroup(offset.node, (offset.index * table.range) + index);

            public Groups<T> Groups()
            {
                int start = offset.index * table.range;
                return new Groups<T>(table.GetGroups(offset.node, Enumerable.Range(start, table.range)));
            }

            public Groups<T> Groups(IEnumerable<int> indexes)
            {
                int start = offset.index * table.range;
                return new Groups<T>(table.GetGroups(offset.node, indexes.Select(i => start + i)));
            }
        }
    }
}