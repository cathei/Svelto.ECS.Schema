using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// IndexTag generates Index and Component pair.
    /// </summary>
    /// <typeparam name="TTag">Type to ensure uniqueness. It can be done with TSelf, but IsUnmanagedEx is on the way</typeparam>
    /// <typeparam name="TKey">The key type you'd like component to hold</typeparam>
    public interface IIndexedRow<TTag, TKey> :
            IIndexableRow<TKey, IIndexedRow<TTag, TKey>.Component>,
            IReactiveRow<IIndexedRow<TTag, TKey>, IIndexedRow<TTag, TKey>.Component>
        where TTag : unmanaged, IIndexedRow<TTag, TKey>.ITag
        where TKey : unmanaged
    {
        public interface ITag {}

        // simple implementation to force index update
        public struct Component : IIndexableComponent<TKey>
        {
            public EGID ID { get; set; }

            private TKey _value;

            public TKey Value => _value;

            // constructors should be only called when building entity
            public Component(in TKey value) : this()
            {
                _value = value;
            }

            public void Update(IndexedDB indexedDB, in TKey value)
            {
                var oldValue = _value;
                _value = value;

                // propagate to indexes
                indexedDB.NotifyKeyUpdate<IIndexedRow<TTag, TKey>, TKey, Component>(ref this, oldValue, value);
            }
        }

        public sealed class Index : IndexBase<IIndexedRow<TTag, TKey>, TKey, Component> { }

        public sealed class Memo : MemoBase<IIndexedRow<TTag, TKey>, Component> { }
    }
}
