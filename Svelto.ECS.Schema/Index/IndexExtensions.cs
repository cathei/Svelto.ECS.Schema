using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static void Update<TComponent>(
                this IndexedDB indexedDB, ref TComponent component)
            where TComponent : unmanaged, IIndexableComponent
        {
            component.UpdateIndex<TComponent>(indexedDB);
        }

        public static void Update<TComponent, TKey>(
                this IndexedDB indexedDB, ref TComponent component, in TKey key)
            where TComponent : unmanaged, IIndexableComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            component.key = key;
            component.UpdateIndex<TComponent>(indexedDB);
        }

        public static void Update<TComponent, TKey>(
                this IndexedDB indexedDB, ref TComponent component, in EnumKey<TKey> key)
            where TComponent : unmanaged, IIndexableComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            component.key = key;
            component.UpdateIndex<TComponent>(indexedDB);
        }
    }
}
