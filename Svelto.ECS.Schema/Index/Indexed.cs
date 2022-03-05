using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IIndexableComponent : IEntityComponent, INeedEGID { }

    public interface IIndexableComponent<TKey> : IIndexableComponent
    {
        TKey Key { get; }
    }

    public interface IKeyEquatable<TSelf>
        where TSelf : IKeyEquatable<TSelf>
    {
        bool KeyEquals(in TSelf other);
    }
}

namespace Svelto.ECS.Schema
{
    public interface IIndexKey<TSelf> : IKeyEquatable<TSelf>
        where TSelf : IIndexKey<TSelf>
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
