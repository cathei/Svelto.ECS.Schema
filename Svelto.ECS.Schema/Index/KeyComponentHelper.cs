using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class KeyComponentHelper<TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        internal interface IHandler
        {
            void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid);
            void Remove(IndexedDB indexedDB, in EntityReference entityReference);
        }

        internal static IHandler Handler;
    }

    internal class KeyComponentHelperImpl<TComponent, TKey> : KeyComponentHelper<TComponent>.IHandler
        where TComponent : unmanaged, IKeyComponent
        where TKey : unmanaged, IEquatable<TKey>
    {
        static KeyComponentHelperImpl()
        {
            KeyComponentHelper<TComponent>.Handler = new KeyComponentHelperImpl<TComponent, TKey>();

            var getMethod = typeof(TComponent).GetProperty(nameof(IKeyComponent<TKey>.key)).GetMethod;
            KeyGetter = (GetterDelegate)Delegate.CreateDelegate(typeof(GetterDelegate), getMethod);
        }

        // just trigger for static constructor
        public static void Warmup() { }

        private static readonly RefWrapperType ComponentType = TypeRefWrapper<TComponent>.wrapper;

        internal delegate TKey GetterDelegate(ref TComponent component);

        internal static readonly GetterDelegate KeyGetter;

        public void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid)
        {
            indexedDB.UpdateIndexableComponent(ComponentType, egid, KeyGetter(ref component));
        }

        public void Remove(IndexedDB indexedDB, in EntityReference reference)
        {
            indexedDB.RemoveIndexableComponent<TKey>(ComponentType, reference);
        }
    }
}
