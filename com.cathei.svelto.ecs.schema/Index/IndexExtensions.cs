using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static void Update<TComponent>(
                this IndexedDB indexedDB, ref TComponent component, in EGID egid)
            where TComponent : unmanaged, IKeyComponent
        {
            KeyComponentHelper<TComponent>.Handler.Update(indexedDB, ref component, egid);
        }

        public static void Update<TComponent, TKey>(
                this IndexedDB indexedDB, ref TComponent component, in EGID egid, in TKey key)
            where TComponent : unmanaged, IKeyComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            component.key = key;
            KeyComponentHelper<TComponent>.Handler.Update(indexedDB, ref component, egid);
        }

        public static void Update<TComponent, TKey>(
                this IndexedDB indexedDB, ref TComponent component, in EGID egid, in EnumKey<TKey> key)
            where TComponent : unmanaged, IKeyComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            component.key = key;
            KeyComponentHelper<TComponent>.Handler.Update(indexedDB, ref component, egid);
        }
    }
}
