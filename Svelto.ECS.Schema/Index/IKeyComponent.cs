using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal static class IndexableComponentHelper<TComponent>
        where TComponent : unmanaged, IKeyComponent
    {
        internal abstract class EngineHandlerBase
        {
            public abstract void AddEngines<TRow>(EnginesRoot enginesRoot, IndexedDB indexedDB)
                where TRow : class, IReactiveRow<TComponent>;
        }

        internal static EngineHandlerBase EngineHandler;

        public static class KeyGetter<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            internal delegate TKey GetterDelegate(ref TComponent component);

            internal static readonly GetterDelegate Getter;

            static KeyGetter()
            {
                EngineHandler = new EngineHandlerImpl();

                var getMethod = typeof(TComponent).GetProperty(nameof(IKeyComponent<TKey>.key)).GetMethod;
                Getter = (GetterDelegate)Delegate.CreateDelegate(typeof(GetterDelegate), getMethod);
            }

            // just trigger for static constructor
            public static void Warmup() { }

            public class EngineHandlerImpl : EngineHandlerBase
            {
                public override void AddEngines<TRow>(EnginesRoot enginesRoot, IndexedDB indexedDB)
                {
                    enginesRoot.AddEngine(new TableIndexingEngine<TRow, TComponent, TKey>(indexedDB));
                }
            }
        }
    }

    public interface IKeyComponent : IEntityComponent
    {
        internal void Warmup<TComponent>() where TComponent : unmanaged, IKeyComponent;
    }
}

namespace Svelto.ECS.Schema
{
    public interface IKeyComponent<TKey> : IKeyComponent
        where TKey : unmanaged, IEquatable<TKey>
    {
        /// <summary>
        /// Key of the index. NOTE: Setting this value is only valid when initializing.
        /// If you want to change this value, call IndexedDB.Update
        /// </summary>
        TKey key { get; set; }

        void IKeyComponent.Warmup<TComponent>()
        {
            IndexableComponentHelper<TComponent>.KeyGetter<TKey>.Warmup();
        }
    }
}