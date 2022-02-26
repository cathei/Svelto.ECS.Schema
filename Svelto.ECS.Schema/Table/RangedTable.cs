using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public abstract class RangedTableBase<TDesc> : ISchemaDefinitionRangedTable
            where TDesc : IEntityDescriptor, new()
        {
            internal readonly Table<TDesc>[] _tables;
            internal readonly int _range;

            public int Range => _range;

            internal RangedTableBase(int range)
            {
                _range = range;

                _tables = new Table<TDesc>[range];
                for (int i = 0; i < range; ++i)
                    _tables[i] = new Table<TDesc>();
            }

            public Table<TDesc> this[int index] => _tables[index];
            public Table<TDesc> Get(int index) => _tables[index];

            public static implicit operator TablesBuilder<TDesc>(RangedTableBase<TDesc> rangedTable)
                => new TablesBuilder<TDesc>(rangedTable._tables.Select(x => x.ExclusiveGroup));

            public static implicit operator Tables<TDesc>(RangedTableBase<TDesc> rangedTable)
                => (TablesBuilder<TDesc>)rangedTable;

            public static TablesBuilder<TDesc> operator +(RangedTableBase<TDesc> a, RangedTableBase<TDesc> b)
                => (TablesBuilder<TDesc>)a + b;

            ISchemaDefinitionTable ISchemaDefinitionRangedTable.GetTable(int index) => _tables[index];
        }
    }

    namespace Definition
    {
        public sealed class RangedTable<TDesc, TIndex> : RangedTableBase<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            internal readonly Func<TIndex, int> _mapper;

            internal RangedTable(int range, Func<TIndex, int> mapper) : base(range)
            {
                _mapper = mapper;
            }

            public Table<TDesc> this[TIndex index] => _tables[_mapper(index)];
            public Table<TDesc> Get(TIndex index) => _tables[_mapper(index)];
        }

        public sealed class RangedTable<TDesc> : RangedTableBase<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            public RangedTable(int range) : base(range) { }
        }
    }
}