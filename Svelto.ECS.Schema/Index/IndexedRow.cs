using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// IndexTag generates Index and Component pair.
    /// </summary>
    /// <typeparam name="TKey">The key type you'd like component to hold</typeparam>
    /// <typeparam name="TTag">Type to ensure uniqueness. It can be done with TSelf, but IsUnmanagedEx is on the way</typeparam>
    public interface IIndexedRow<TKey, TTag> :
            IIndexableRow<TKey, IIndexedRow<TKey, TTag>.Component>,
            IReactiveRow<IIndexedRow<TKey, TTag>.Component>
        where TKey : unmanaged
        where TTag : unmanaged, IIndexedRow<TKey, TTag>.ITag
    {
        public interface ITag {}

        // simple implementation to force index update
        public struct Component : IIndexableComponent<TKey>
        {
            public EGID ID { get; set; }

            internal TKey _value;

            public TKey Value => _value;

            // constructors should be only called when building entity
            public Component(in TKey value) : this()
            {
                _value = value;
            }
        }

        public sealed class Index : IndexBase<IIndexedRow<TKey, TTag>, TKey, Component> { }

        public sealed class Memo : MemoBase<IIndexedRow<TKey, TTag>, Component> { }
    }
}
