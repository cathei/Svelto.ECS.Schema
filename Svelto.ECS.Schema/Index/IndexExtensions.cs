using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static void Update<TC>(
                this IndexedDB indexedDB, ref TC component)
            where TC : unmanaged, IIndexableComponent
        {
            component.UpdateIndex<TC>(indexedDB);
        }

        public static void Update<TC, TK>(
                this IndexedDB indexedDB, ref TC component, in TK key)
            where TC : unmanaged, IIndexableComponent<TK>
            where TK : unmanaged, IEquatable<TK>
        {
            component.key = key;
            component.UpdateIndex<TC>(indexedDB);
        }
    }
}
