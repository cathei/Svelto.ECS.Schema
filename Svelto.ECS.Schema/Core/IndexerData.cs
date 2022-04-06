using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    internal abstract class IndexerData
    { }

    internal sealed class IndexerData<TKey> : IndexerData
        where TKey : unmanaged, IEquatable<TKey>
    {
        internal readonly FasterDictionary<TKey, int> keyToFilterID = new();

        public int Get(in TKey key)
        {
            if (!keyToFilterID.TryGetValue(key, out var filterID))
            {
                filterID = keyToFilterID.count;
                keyToFilterID[key] = filterID;
            }

            return filterID;
        }

        public bool Contains(in TKey key) => keyToFilterID.ContainsKey(key);
    }
}
