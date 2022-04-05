using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class KeyComponentHelper<TComponent>
        where TComponent : unmanaged, IEntityComponent
    {
        internal interface IComponentHandler
        {
            void AddEngines<TRow>(EnginesRoot enginesRoot, IndexedDB indexedDB)
                where TRow : class, IQueryableRow<ResultSet<TComponent>>;

            void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid);
            void Remove(IndexedDB indexedDB, in EGID egid);
        }

        internal static IComponentHandler Handler;
    }

    internal static class KeyComponentHelper<TComponent, TKey>
        where TComponent : unmanaged, IKeyComponent<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        static KeyComponentHelper()
        {
            KeyComponentHelper<TComponent>.Handler = new HandlerImpl();
        }

        private static readonly RefWrapperType ComponentType = TypeRefWrapper<TComponent>.wrapper;

        // just trigger for static constructor
        public static void Warmup() { }

        private class HandlerImpl : KeyComponentHelper<TComponent>.IComponentHandler
        {
            public void AddEngines<TRow>(EnginesRoot enginesRoot, IndexedDB indexedDB)
                where TRow : class, IQueryableRow<ResultSet<TComponent>>
            {
                enginesRoot.AddEngine(new TableIndexingEngine<TRow, TComponent>(indexedDB));
            }

            public void Update(IndexedDB indexedDB, ref TComponent component, in EGID egid)
            {
                indexedDB.UpdateIndexableComponent(ComponentType, egid, component.key);
            }

            public void Remove(IndexedDB indexedDB, in EGID egid)
            {
                indexedDB.RemoveIndexableComponent<TKey>(ComponentType, egid);
            }
        }
    }
}
