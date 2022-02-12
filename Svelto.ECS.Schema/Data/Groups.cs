using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public struct Groups<T> where T : IEntityDescriptor
    {
        public IEnumerable<ExclusiveGroupStruct> items;
        private FasterList<ExclusiveGroupStruct> builtGroups;

        public Groups(IEnumerable<ExclusiveGroupStruct> items)
        {
            this.items = items;
            builtGroups = null;
        }

        public FasterList<ExclusiveGroupStruct> Build()
        {
            if (builtGroups != null)
                return builtGroups;

            builtGroups = items.ToFasterList();
            return builtGroups;
        }

        public static Groups<T> operator+(Groups<T> a, Groups<T> b)
        {
            return new Groups<T>(a.items.Concat(b.items));
        }

        public static Groups<T> operator+(Groups<T> a, Group<T> b)
        {
            return new Groups<T>(a.items.Append(b.exclusiveGroup));
        }

        public static Groups<T> operator+(Group<T> a, Groups<T> b)
        {
            return new Groups<T>(b.items.Prepend(a.exclusiveGroup));
        }
    }
}