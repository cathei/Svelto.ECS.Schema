using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IKeyComponent : IEntityComponent
    {
        internal void Warmup<TComponent>() where TComponent : unmanaged, IKeyComponent;
    }

    public interface IKeyedRow<TComponent> : IEntityRow<TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }
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
            KeyComponentHelperImpl<TComponent, TKey>.Warmup();
        }
    }
}
