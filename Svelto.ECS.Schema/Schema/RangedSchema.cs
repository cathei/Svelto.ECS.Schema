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

            public delegate TRanged RangedTableSelector<out TRanged>(TSchema schema);

            public TablesBuilder<TDesc> Combine<TDesc>(Func<TSchema, Table<TDesc>> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(Enumerable.Range(0, _range), selector);

            public TablesBuilder<TDesc> Combine<TDesc>(TablesBuilderSelector<TDesc> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(Enumerable.Range(0, _range), selector);

            public TablesBuilder<TDesc> Combine<TDesc>(RangedTableSelector<RangedTableBase<TDesc>> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(Enumerable.Range(0, _range), selector);

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<int> indexes, Func<TSchema, Table<TDesc>> selector)
                    where TDesc : IEntityDescriptor, new()
                => new TablesBuilder<TDesc>(indexes.Select(x => selector(_schemas[x]).ExclusiveGroup));

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<int> indexes, TablesBuilderSelector<TDesc> selector)
                    where TDesc : IEntityDescriptor, new()
                => new TablesBuilder<TDesc>(indexes.SelectMany(x => selector(_schemas[x]).items));

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<int> indexes, RangedTableSelector<RangedTableBase<TDesc>> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(indexes, x => (TablesBuilder<TDesc>)selector(x));

            // public TablesBuilder<TDesc> Combine<TDesc, TIndex>(IEnumerable<int> indexes, Func<TSchema, RangedTable<TDesc, TIndex>> selector)
            //         where TDesc : IEntityDescriptor, new()
            //     => Combine(indexes, x => (TablesBuilder<TDesc>)selector(x));

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

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<TIndex> indexes, Func<TSchema, Table<TDesc>> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(indexes.Select(_mapper), selector);

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<TIndex> indexes, TablesBuilderSelector<TDesc> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(indexes.Select(_mapper), selector);

            public TablesBuilder<TDesc> Combine<TDesc>(IEnumerable<TIndex> indexes, RangedTableSelector<RangedTableBase<TDesc>> selector)
                    where TDesc : IEntityDescriptor, new()
                => Combine(indexes.Select(_mapper), selector);

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