using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly struct Group<T> where T : IEntityDescriptor
    {
        public readonly ExclusiveGroupStruct exclusiveGroup;

        public Group(ExclusiveGroupStruct group)
        {
            exclusiveGroup = group;
        }

        public static Groups<T> operator+(Group<T> a, Group<T> b)
        {
            return new Groups<T>(new [] { a.exclusiveGroup, b.exclusiveGroup });
        }

        public static implicit operator ExclusiveGroupStruct(Group<T> group) => group.exclusiveGroup;
    }
}