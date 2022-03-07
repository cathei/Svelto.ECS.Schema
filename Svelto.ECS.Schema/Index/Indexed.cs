using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // we need EGID constraints because it requires INeedEGID
    // it won't be necessary when Svelto update it's filter utility functions
    public interface IIndexableComponent : IEntityComponent, INeedEGID { }

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

    public interface IIndexableRow<TComponent> :
            IReactiveRow<TComponent>,
            IQueryableRow<IndexableResultSet<TComponent>>
        where TComponent : unmanaged, IEntityComponent, INeedEGID
    { }

    public interface IIndexableComponent<TKey> : IIndexableComponent
    {
        TKey Key { get; }
    }

    public interface IKeyEquatable<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        TKey Key { get; set; }
    }
}

namespace Svelto.ECS.Schema
{
    public interface IIndexKey<TTag> : IKeyEquatable<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    { }

    public struct Indexed<TKey> : IIndexableComponent<TKey>
        where TKey : unmanaged, IIndexKey<TKey>
    {
        public EGID ID { get; set; }

        internal TKey _key;

        public TKey Key => _key;

        // should be only called for initialization
        public Indexed(in TKey key) : this()
        {
            _key = key;
        }
    }

    public interface IIndexedRow<TKey> : IIndexableRow<Indexed<TKey>>
        where TKey : unmanaged, IIndexKey<TKey>
    { }
}
