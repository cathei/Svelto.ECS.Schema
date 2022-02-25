using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    public readonly struct MultiIndexKey<T1, T2> : IKeyEquatable<MultiIndexKey<T1, T2>>
        where T1 : unmanaged, IEntityIndexKey<T1>
        where T2 : unmanaged, IEntityIndexKey<T2>
    {
        public readonly T1 key1;
        public readonly T2 key2;

        public bool Equals(MultiIndexKey<T1, T2> other)
        {
            if (!key1.Equals(other.key1))
                return false;

            if (!key2.Equals(other.key2))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return key1.GetHashCode() ^ key2.GetHashCode();
        }
    }
}