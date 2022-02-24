using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Shard<T> : IEntitySchemaShard
        where T : class, IEntitySchema, new()
    {
        internal readonly T[] _schemas;
        internal readonly int _range;

        public int Range => _range;

        public Type InnerType => typeof(T);

        public Shard(int range = 1)
        {
            _range = range;

            _schemas = new T[range];
            for (int i = 0; i < range; ++i)
                _schemas[i] = new T();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Schema(int index = 0) => GetSchema(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShardsBuilder<T> Schemas() => new ShardsBuilder<T>(GetSchemas(Enumerable.Range(0, _range)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShardsBuilder<T> Schemas(IEnumerable<int> indexes) => new ShardsBuilder<T>(GetSchemas(indexes));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetSchema(int index)
        {
            return _schemas[index];
        }

        private IEnumerable<T> GetSchemas(IEnumerable<int> indexes)
        {
            foreach (int i in indexes)
                yield return _schemas[i];
        }

        object IEntitySchemaShard.GetSchema(int index) => GetSchema(index);
    }
}