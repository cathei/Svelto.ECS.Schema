using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // we need EGID constraints because it requires INeedEGID
    // it won't be necessary when Svelto update it's filter utility functions
    public interface IIndexableComponent : IEntityComponent, INeedEGID
    {
        internal void UpdateIndex<TComponent>(IndexedDB indexedDB) where TComponent : unmanaged, IIndexableComponent;
        internal void RemoveFromIndex<TComponent>(IndexedDB indexedDB) where TComponent : unmanaged, IIndexableComponent;
    }

    public struct IndexableResultSet<T> : IResultSet<T>
        where T : unmanaged, IEntityComponent, INeedEGID
    {
        public int count { get; set; }

        public NB<T> component;

        public void Init(in EntityCollection<T> buffers)
        {
            (component, count) = buffers;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public interface IIndexableRow<TComponent> :
            IReactiveRow<TComponent>, IQueryableRow<IndexableResultSet<TComponent>>
        where TComponent : unmanaged, IEntityComponent, INeedEGID
    { }

    public interface IIndexableComponent<TKey> : IIndexableComponent
        where TKey : unmanaged, IEquatable<TKey>
    {
        /// <summary>
        /// Key of the index. NOTE: Setting this value is only valid when initializing.
        /// If you want to change this value, call IndexedDB.Update
        /// </summary>
        TKey key { get; set; }

        void IIndexableComponent.UpdateIndex<TComponent>(IndexedDB indexedDB)
        {
            indexedDB.UpdateIndexableComponent<TComponent, TKey>(ID, key);
        }

        void IIndexableComponent.RemoveFromIndex<TComponent>(IndexedDB indexedDB)
        {
            indexedDB.RemoveIndexableComponent<TComponent, TKey>(ID);
        }
    }
}
