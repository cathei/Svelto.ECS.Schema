using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    /// <summary>
    /// IndexTag generates Index and Component pair.
    /// </summary>
    /// <typeparam name="TValue">The value type you'd like component to hold</typeparam>
    /// <typeparam name="TUnique">Type to ensure uniqueness. It can be done with TSelf, but IsUnmanagedEx is on the way</typeparam>
    public abstract class IndexTag<TValue, TUnique>
        where TValue : unmanaged
        where TUnique : unmanaged, IndexTag<TValue, TUnique>.IUnique
    {
        public interface IUnique {}

        // simple implementation to force index update
        public struct Component : IIndexedComponent<TValue>
        {
            public EGID ID { get; set; }

            private TValue _value;

            public TValue Value => _value;

            // constructors should be only called when building entity
            public Component(in TValue value) : this()
            {
                _value = value;
            }

            public void Update(IndexedDB indexedDB, in TValue value)
            {
                var oldValue = _value;
                _value = value;

                // propagate to indexes
                indexedDB.NotifyKeyUpdate(ref this, oldValue, value);
            }
        }

        public sealed class Index : IndexBase<TValue, Component>
        {

        }
    }
}
