using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static void Update<TKey>(
                this IndexedDB indexedDB, ref Indexed<TKey> component, in TKey key)
            where TKey : unmanaged, IIndexKey<TKey>
        {
            var oldKey = component._key;
            component._key = key;

            // propagate to indexes
            indexedDB.NotifyKeyUpdate(ref component, oldKey, key);
        }
    }
}
