using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Table<T> : IEntitySchemaTable where T : IEntityDescriptor, new()
    {
        private readonly ExclusiveGroup _exclusiveGroup;
        private readonly int _range;

        public ref readonly ExclusiveGroup ExclusiveGroup => ref _exclusiveGroup;
        public int Range => _range;

        public Table(int range = 1)
        {
            _range = range;
            _exclusiveGroup = new ExclusiveGroup((ushort)range);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Group<T> Group(int index = 0) => new Group<T>(GetGroup(index));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupsBuilder<T> Groups() => new GroupsBuilder<T>(GetGroups(Enumerable.Range(0, _range)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupsBuilder<T> Groups(IEnumerable<int> indexes) => new GroupsBuilder<T>(GetGroups(indexes));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ExclusiveGroupStruct GetGroup(int index)
        {
            return _exclusiveGroup + (ushort)index;
        }

        private IEnumerable<ExclusiveGroupStruct> GetGroups(IEnumerable<int> indexes)
        {
            foreach (int i in indexes)
                yield return _exclusiveGroup + (ushort)i;
        }
    }
}