using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public class RangedSchemaBase<TSchema> : IEntitySchemaShard
            where TSchema : class, IEntitySchema, new()
        {
            internal readonly TSchema[] _schemas;
            internal readonly int _range;

            public Type InnerType => typeof(TSchema);
            public int Range => _range;

            internal RangedSchemaBase(int range)
            {
                _range = range;

                _schemas = new TSchema[range];
                for (int i = 0; i < range; ++i)
                    _schemas[i] = new TSchema();
            }

            public delegate TablesBuilder<TDesc> TablesBuilderSelector<TDesc>(TSchema schema)
                where TDesc : IEntityDescriptor, new();

            public TablesBuilder<TDesc> Combine<TDesc>(Func<TSchema, Table<TDesc>> selector)
                where TDesc : IEntityDescriptor, new()
            {
                return Combine(Enumerable.Range(0, _range), selector);
            }

            public TablesBuilder<TDesc> Combine<TDesc>(TablesBuilderSelector<TDesc> selector)
                where TDesc : IEntityDescriptor, new()
            {
                return Combine(Enumerable.Range(0, _range), selector);
            }

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<int> indexes, Func<TSchema, Table<TDesc>> selector)
                where TDesc : IEntityDescriptor, new()
            {
                return new TablesBuilder<TDesc>(indexes.Select(x => selector(_schemas[x]).ExclusiveGroupStruct));
            }

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<int> indexes, TablesBuilderSelector<TDesc> selector)
                where TDesc : IEntityDescriptor, new()
            {
                return new TablesBuilder<TDesc>(indexes.SelectMany(x => selector(_schemas[x]).items));
            }

            public TSchema this[int index] => _schemas[index];

            object IEntitySchemaShard.GetSchema(int index) => _schemas[index];
        }
    }

    namespace Definition
    {
        public sealed class Ranged<TSchema, TIndex> : Internal.RangedSchemaBase<TSchema>
            where TSchema : class, IEntitySchema, new()
        {
            internal readonly Func<TIndex, int> _mapper;

            internal Ranged(int range, Func<TIndex, int> mapper) : base(range)
            {
                _mapper = mapper;
            }

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<TIndex> indexes, Func<TSchema, Table<TDesc>> selector)
                where TDesc : IEntityDescriptor, new()
            {
                return Combine(indexes.Select(_mapper), selector);
            }

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<TIndex> indexes, TablesBuilderSelector<TDesc> selector)
                where TDesc : IEntityDescriptor, new()
            {
                return Combine(indexes.Select(_mapper), selector);
            }

            public TSchema this[TIndex index] => _schemas[_mapper(index)];
        }

        public sealed class Ranged<TSchema> : Internal.RangedSchemaBase<TSchema>
            where TSchema : class, IEntitySchema, new()
        {
            public Ranged(int range) : base(range) { }
        }
    }
}