using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // it is forced to used this becuase FasterDictionary requires IEquatable<T>
    internal readonly struct KeyWrapper<T> : IEquatable<KeyWrapper<T>>
        where T : unmanaged, IKeyEquatable<T>
    {
        private readonly T _value;
        private readonly int _hashcode;

        public KeyWrapper(in T value)
        {
            _value = value;
            _hashcode = _value.KeyHashCode();
        }

        public bool Equals(KeyWrapper<T> other) => _value.KeyEquals(other._value);

        public override bool Equals(object obj) => obj is KeyWrapper<T> other && Equals(other);

        public override int GetHashCode() => _hashcode;
    }
}