using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public abstract class RangedTableBase<TDesc> : IEntitySchemaElement
            where TDesc : IEntityDescriptor, new()
        {
            internal readonly Table<TDesc>[] _tables;
            internal readonly int _range;

            public Type InnerType => typeof(TDesc);
            public int Range => _range;

            internal RangedTableBase(int range)
            {
                _range = range;

                _tables = new Table<TDesc>[range];
                for (int i = 0; i < range; ++i)
                    _tables[i] = new Table<TDesc>();
            }

            public Table<TDesc> this[int index] => _tables[index];

            public static implicit operator TablesBuilder<TDesc>(RangedTableBase<TDesc> rangedTable)
                => new TablesBuilder<TDesc>(rangedTable._tables.Select(x => x.ExclusiveGroupStruct));

            public static implicit operator Tables<TDesc>(RangedTableBase<TDesc> rangedTable)
                => (TablesBuilder<TDesc>)rangedTable;
        }
    }

    namespace Definition
    {
        public sealed class RangedTable<TDesc, TIndex> : Internal.RangedTableBase<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            internal readonly Func<TIndex, int> _mapper;

            internal RangedTable(int range, Func<TIndex, int> mapper) : base(range)
            {
                _mapper = mapper;
            }

            public Table<TDesc> this[TIndex index] => _tables[_mapper(index)];
        }

        public sealed class RangedTable<TDesc> : Internal.RangedTableBase<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            public RangedTable(int range) : base(range) { }
        }
    }
}