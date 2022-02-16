using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly ref struct GroupsBuilder<T> where T : IEntityDescriptor, new()
    {
        public readonly IEnumerable<ExclusiveGroupStruct> items;

        public GroupsBuilder(IEnumerable<ExclusiveGroupStruct> items)
        {
            this.items = items;
        }

        public Groups<T> Build()
        {
            return new Groups<T>(items.ToFasterList());
        }

        public static GroupsBuilder<T> operator+(GroupsBuilder<T> a, GroupsBuilder<T> b)
        {
            return new GroupsBuilder<T>(a.items.Concat(b.items));
        }

        public static GroupsBuilder<T> operator+(GroupsBuilder<T> a, in Group<T> b)
        {
            return new GroupsBuilder<T>(a.items.Append(b.exclusiveGroup));
        }

        public static GroupsBuilder<T> operator+(in Group<T> a, GroupsBuilder<T> b)
        {
            return new GroupsBuilder<T>(b.items.Prepend(a.exclusiveGroup));
        }

        public static implicit operator Groups<T>(in GroupsBuilder<T> builder) => builder.Build();
    }
}