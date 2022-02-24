using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly struct ShardsBuilder<T> where T : class, IEntitySchema
    {
        private readonly IEnumerable<T> _schemas;

        public delegate TablesBuilder<TDesc> TablesBuilderSelector<TDesc>(T shard) where TDesc : IEntityDescriptor, new();

        internal ShardsBuilder(IEnumerable<T> schemas)
        {
            _schemas = schemas;
        }

        public ShardsBuilder<TOut> Combine<TOut>(Func<T, TOut> selector) where TOut : class, IEntitySchema
        {
            return new ShardsBuilder<TOut>(_schemas.Select(selector));
        }

        public ShardsBuilder<TOut> Combine<TOut>(Func<T, ShardsBuilder<TOut>> selector) where TOut : class, IEntitySchema
        {
            return new ShardsBuilder<TOut>(Unpack(selector));
        }

        public TablesBuilder<TDesc> Combine<TDesc>(Func<T, Table<TDesc>> selector) where TDesc : IEntityDescriptor, new()
        {
            return new TablesBuilder<TDesc>(_schemas.Select(x => selector(x)._exclusiveGroup));
        }

        public TablesBuilder<TDesc> Combine<TDesc>(TablesBuilderSelector<TDesc> selector) where TDesc : IEntityDescriptor, new()
        {
            return new TablesBuilder<TDesc>(Unpack(selector));
        }

        // just SelectMany...
        internal IEnumerable<TOut> Unpack<TOut>(Func<T, ShardsBuilder<TOut>> selector) where TOut : class, IEntitySchema
        {
            foreach (var schema in _schemas)
            {
                foreach (var outSchema in selector(schema)._schemas)
                    yield return outSchema;
            }
        }

        internal IEnumerable<ExclusiveGroupStruct> Unpack<TDesc>(TablesBuilderSelector<TDesc> selector) where TDesc : IEntityDescriptor, new()
        {
            foreach (var schema in _schemas)
            {
                foreach (var group in selector(schema).items)
                    yield return group;
            }
        }
    }
}