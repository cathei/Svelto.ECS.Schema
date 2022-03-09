using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        // public static void Update<TComponent>(
        //         this IndexedDB indexedDB, ref TComponent component)
        //     where TComponent : unmanaged, IIndexableComponent
        // {
        //     indexedDB.UpdateIndexableComponent(, component.ID,
        //         IndexComponentReflection<TComponent>.KeyGetter<TK>.Getter(ref component));
        // }

        public static void Update<TComponent, TKey>(
                this IndexedDB indexedDB, ref TComponent component, in TKey key)
            where TComponent : unmanaged, IIndexableComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            component.key = key;
            indexedDB.UpdateIndexableComponent(
                TypeRefWrapper<TComponent>.wrapper, component.ID, component.key);
        }

        public static void Update<TComponent, TKey>(
                this IndexedDB indexedDB, ref TComponent component, in EnumKey<TKey> key)
            where TComponent : unmanaged, IIndexableComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            component.key = key;
            indexedDB.UpdateIndexableComponent(
                TypeRefWrapper<TComponent>.wrapper, component.ID, component.key);
        }
    }
}
