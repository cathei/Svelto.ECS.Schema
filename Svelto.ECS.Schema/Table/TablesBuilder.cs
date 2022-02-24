using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    public readonly ref struct TablesBuilder<T> where T : IEntityDescriptor, new()
    {
        public readonly IEnumerable<ExclusiveGroupStruct> items;

        public TablesBuilder(IEnumerable<ExclusiveGroupStruct> items)
        {
            this.items = items;
        }

        public Tables<T> Build()
        {
            return new Tables<T>(items.ToFasterList());
        }

        public static TablesBuilder<T> operator+(TablesBuilder<T> a, TablesBuilder<T> b)
        {
            return new TablesBuilder<T>(a.items.Concat(b.items));
        }

        public static TablesBuilder<T> operator+(TablesBuilder<T> a, Table<T> b)
        {
            return new TablesBuilder<T>(a.items.Append(b.ExclusiveGroupStruct));
        }

        public static TablesBuilder<T> operator+(Table<T> a, TablesBuilder<T> b)
        {
            return new TablesBuilder<T>(b.items.Prepend(a.ExclusiveGroupStruct));
        }

        public static implicit operator Tables<T>(in TablesBuilder<T> builder) => builder.Build();
    }
}