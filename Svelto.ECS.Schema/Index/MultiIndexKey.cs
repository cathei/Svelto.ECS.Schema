using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    // I'm experimenting with this but unlikely to happen so far
    // To have multi key index we need reverse mapping (entity to filter)
    // and also needs to be able to inspect other component
    // so overall it may be possible but expensive operation for now..
    // what about MultiTableIndexingEngine<T1> caches all the group move while submission..?
    public readonly struct MultiIndexKey<T1, T2> : IKeyEquatable<MultiIndexKey<T1, T2>>
        where T1 : unmanaged, IEntityIndexKey<T1>
        where T2 : unmanaged, IEntityIndexKey<T2>
    {
        // it has to be nullable to ensure add entity order
        public readonly T1? key1;
        public readonly T2? key2;

        public bool Equals(MultiIndexKey<T1, T2> other)
        {
            if (key1.HasValue != other.key1.HasValue || (key1.HasValue && !key1.Value.Equals(other.key1.Value)))
                return false;

            if (key2.HasValue != other.key2.HasValue || (key2.HasValue && !key2.Value.Equals(other.key2.Value)))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return key1.GetHashCode() ^ key2.GetHashCode();
        }
    }

    public sealed class Index<T1, T2> : IndexBase<MultiIndexKey<T1, T2>>
        where T1 : unmanaged, IEntityIndexKey<T1>
        where T2 : unmanaged, IEntityIndexKey<T2>
    {

    }
}