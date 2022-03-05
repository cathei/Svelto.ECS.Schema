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
        public abstract class RangedSchemaBase<TSchema> : ISchemaDefinitionRangedSchema
            where TSchema : class, IEntitySchema, new()
        {
            internal readonly TSchema[] _schemas;
            internal readonly int _range;

            public int Range => _range;

            internal RangedSchemaBase(int range)
            {
                _range = range;

                _schemas = new TSchema[range];
                for (int i = 0; i < range; ++i)
                    _schemas[i] = new TSchema();
            }

            public TablesBuilder<TRow> Combine<TRow>(
                    Func<TSchema, IEntityTablesBuilder<TRow>> selector)
                where TRow : class, IEntityRow
            {
                return Combine(Enumerable.Range(0, _range), selector);
            }

            public TablesBuilder<TRow> Combine<TRow>(
                    IEnumerable<int> indexes, Func<TSchema, IEntityTablesBuilder<TRow>> selector)
                where TRow : class, IEntityRow
            {
                return new TablesBuilder<TRow>(indexes.SelectMany(i => selector(_schemas[i]).Tables));
            }

            public TSchema this[int index] => _schemas[index];
            public TSchema Get(int index) => _schemas[index];

            IEntitySchema ISchemaDefinitionRangedSchema.GetSchema(int index) => _schemas[index];
        }
    }

    namespace Definition
    {
        public sealed class Ranged<TSchema, TIndex> : RangedSchemaBase<TSchema>
            where TSchema : class, IEntitySchema, new()
        {
            internal readonly Func<TIndex, int> _mapper;

            internal Ranged(int range, Func<TIndex, int> mapper) : base(range)
            {
                _mapper = mapper;
            }

            internal Ranged(TIndex range, Func<TIndex, int> mapper) : base(mapper(range))
            {
                _mapper = mapper;
            }

            public TablesBuilder<TRow> Combine<TRow>(
                    IEnumerable<TIndex> indexes, Func<TSchema, IEntityTablesBuilder<TRow>> selector)
                where TRow : class, IEntityRow
            {
                return Combine(indexes.Select(_mapper), selector);
            }

            public TSchema this[TIndex index] => _schemas[_mapper(index)];
            public TSchema Get(TIndex index) => _schemas[_mapper(index)];
        }

        public sealed class Ranged<TSchema> : RangedSchemaBase<TSchema>
            where TSchema : class, IEntitySchema, new()
        {
            public Ranged(int range) : base(range) { }
        }
    }
}