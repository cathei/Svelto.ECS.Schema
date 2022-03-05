using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class IndexExtensions
    {
        public static void Update<TData, TKey>(
                this IndexedDB indexedDB, ref Indexed<TData> component, in TKey value)
            where TData : unmanaged, IIndexedData<TKey>
            where TKey : unmanaged
        {
            var oldValue = component._data.Key;
            component._data.Key = value;

            // propagate to indexes
            indexedDB.NotifyKeyUpdate(ref component, oldValue, value);
        }
    }
}
