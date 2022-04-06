using System.Collections.Generic;
using System.Threading;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    /// <summary>
    /// this is pooled object and contains all information about current query
    /// disposing any object in select chain will return this object to pool
    /// </summary>
    public sealed class ResultSetQueryConfig
    {
        internal bool isReturned = false;

        internal IndexedDB indexedDB;

        internal FasterDictionary<int, int> pkToValue = new();
        internal FasterList<EntityFilterCollection> filters = new();
        internal HashSet<uint> selectedEntityIDs = new();

        internal SharedSveltoDictionaryNative<ExclusiveGroupStruct, ExclusiveGroupStruct> temporaryGroups = new(0);
        internal NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> temporaryFilters = new(NativeDynamicArray.Alloc<EntityFilterCollection.GroupFilters>());
        internal NativeDynamicArrayCast<uint> temporaryEntityIndices = new(NativeDynamicArray.Alloc<uint>());

        internal static ThreadLocal<Stack<ResultSetQueryConfig>> Pool = new(() => new());

        internal static ResultSetQueryConfig Use()
        {
            if (Pool.Value.TryPop(out var result))
            {
                result.isReturned = false;
                return result;
            }

            return new ResultSetQueryConfig();
        }

        internal static void Return(ResultSetQueryConfig config)
        {
            if (config.isReturned)
                return;

            config.isReturned = true;
            config.indexedDB = null;
            config.pkToValue.FastClear();
            config.filters.FastClear();
            config.selectedEntityIDs.Clear();

            config.temporaryGroups.FastClear();
            config.temporaryFilters.Clear();
            config.temporaryEntityIndices.Clear();

            Pool.Value.Push(config);
        }
    }
}