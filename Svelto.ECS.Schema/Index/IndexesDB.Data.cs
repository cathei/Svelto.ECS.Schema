using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class IndexesDB
    {
        internal struct IndexerGroupData
        {
            public ExclusiveGroupStruct group;
            public FilterGroup filter;
        }

        public struct IndexerSetData
        {
            internal FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> groups;

            public void Clear()
            {
                for (int i = 0; i < groups.count; ++i)
                    groups.unsafeValues[i].filter.Clear();
            }
        }

        internal abstract class IndexerData {}

        internal sealed class IndexerData<TKey> : IndexerData
            where TKey : unmanaged, IKeyEquatable<TKey>
        {
            private readonly FasterDictionary<IKeyEquatable<TKey>.Wrapper, IndexerSetData> keyToGroups
                = new FasterDictionary<IKeyEquatable<TKey>.Wrapper, IndexerSetData>();

            public ref IndexerSetData CreateOrGet(in TKey key)
            {
                return ref keyToGroups.GetOrCreate(new IKeyEquatable<TKey>.Wrapper(key), () => new IndexerSetData
                {
                    groups = new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>()
                });
            }

            public bool TryGetValue(in TKey key, out IndexerSetData result)
            {
                return keyToGroups.TryGetValue(new IKeyEquatable<TKey>.Wrapper(key), out result);
            }
        }
    }
}
