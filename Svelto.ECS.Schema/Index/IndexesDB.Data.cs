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
            public readonly struct KeyWrapper : IEquatable<KeyWrapper>
            {
                private readonly TKey _value;
                private readonly int _hashcode;

                public KeyWrapper(TKey value)
                {
                    _value = value;
                    _hashcode = _value.GetHashCode();
                }

                // this uses IKeyEquatable<T>.Equals
                public bool Equals(KeyWrapper other) => _value.Equals(other._value);

                public override bool Equals(object obj) => obj is IndexerData<TKey> other && Equals(other);

                public override int GetHashCode() => _hashcode;

                public static implicit operator TKey(KeyWrapper t) => t._value;
            }

            private readonly FasterDictionary<KeyWrapper, IndexerSetData> keyToGroups
                = new FasterDictionary<KeyWrapper, IndexerSetData>();

            public ref IndexerSetData CreateOrGet(in TKey key)
            {
                return ref keyToGroups.GetOrCreate(new KeyWrapper(key), () => new IndexerSetData
                {
                    groups = new FasterDictionary<ExclusiveGroupStruct, IndexerGroupData>()
                });
            }

            public bool TryGetValue(in TKey key, out IndexerSetData result)
            {
                return keyToGroups.TryGetValue(new KeyWrapper(key), out result);
            }
        }
    }
}
