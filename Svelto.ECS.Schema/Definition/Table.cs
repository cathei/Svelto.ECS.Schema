using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Table<T> : IEntitySchemaTable where T : IEntityDescriptor, new()
    {
        internal readonly ExclusiveGroup exclusiveGroup;
        internal readonly int range;

        public ref readonly ExclusiveGroup ExclusiveGroup => ref exclusiveGroup;
        public int Range => range;

        public Table(int range = 1)
        {
            this.range = range;
            exclusiveGroup = new ExclusiveGroup((ushort)range);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Group<T> Group(int index = 0) => new Group<T>(GetGroup(index));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupsBuilder<T> Groups() => new GroupsBuilder<T>(GetGroups(Enumerable.Range(0, range)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupsBuilder<T> Groups(IEnumerable<int> indexes) => new GroupsBuilder<T>(GetGroups(indexes));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ExclusiveGroupStruct GetGroup(int index)
        {
            return exclusiveGroup + (ushort)index;
        }

        private IEnumerable<ExclusiveGroupStruct> GetGroups(IEnumerable<int> indexes)
        {
            foreach (int i in indexes)
                yield return exclusiveGroup + (ushort)i;
        }
    }
}