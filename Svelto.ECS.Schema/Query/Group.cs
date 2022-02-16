using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly partial struct Group<T> where T : IEntityDescriptor, new()
    {
        public readonly ExclusiveGroupStruct exclusiveGroup;

        public Group(in ExclusiveGroupStruct group)
        {
            exclusiveGroup = group;
        }

        public static GroupsBuilder<T> operator+(in Group<T> a, in Group<T> b)
        {
            return new GroupsBuilder<T>(new [] { a.exclusiveGroup, b.exclusiveGroup });
        }

        public static implicit operator ExclusiveGroupStruct(in Group<T> group) => group.exclusiveGroup;
    }
}