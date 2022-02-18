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

        public delegate GroupsBuilder<TDesc> GroupsBuilderSelector<TDesc>(T shard) where TDesc : IEntityDescriptor, new();

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

        public GroupsBuilder<TDesc> Combine<TDesc>(Func<T, Group<TDesc>> selector) where TDesc : IEntityDescriptor, new()
        {
            return new GroupsBuilder<TDesc>(_schemas.Select(x => selector(x).exclusiveGroup));
        }

        public GroupsBuilder<TDesc> Combine<TDesc>(GroupsBuilderSelector<TDesc> selector) where TDesc : IEntityDescriptor, new()
        {
            return new GroupsBuilder<TDesc>(Unpack(selector));
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

        internal IEnumerable<ExclusiveGroupStruct> Unpack<TDesc>(GroupsBuilderSelector<TDesc> selector) where TDesc : IEntityDescriptor, new()
        {
            foreach (var schema in _schemas)
            {
                foreach (var group in selector(schema).items)
                    yield return group;
            }
        }
    }
}