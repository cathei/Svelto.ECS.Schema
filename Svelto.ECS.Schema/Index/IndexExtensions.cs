using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static void Update<TKey, TTag>(this IndexedDB indexedDB,
                ref IIndexedRow<TKey, TTag>.Component component, in TKey value)
            where TKey : unmanaged
            where TTag : unmanaged, IIndexedRow<TKey, TTag>.ITag
        {
            var oldValue = component._value;
            component._value = value;

            // propagate to indexes
            indexedDB.NotifyKeyUpdate<IIndexedRow<TKey, TTag>, TKey, IIndexedRow<TKey, TTag>.Component>(
                ref component, oldValue, value);
        }
    }
}
