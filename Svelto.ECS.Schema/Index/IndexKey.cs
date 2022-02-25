using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        // This is not IEquatable because I want to keep it simple.
        // Without verbosely override object.Equals and == operator etc.
        // But if user wants they can always implement
        // Also prevents primitive type being used as index key
        public interface IKeyEquatable<T>
            where T : unmanaged, IKeyEquatable<T>
        {
            bool Equals(T other);

            internal readonly struct Wrapper : IEquatable<Wrapper>
            {
                private readonly T _value;
                private readonly int _hashcode;

                public Wrapper(T value)
                {
                    _value = value;
                    _hashcode = _value.GetHashCode();
                }

                // this uses IKeyEquatable<T>.Equals
                public bool Equals(Wrapper other) => _value.Equals(other._value);

                public override bool Equals(object obj) => obj is Wrapper other && Equals(other);

                public override int GetHashCode() => _hashcode;

                public static implicit operator T(Wrapper t) => t._value;
            }
        }
    }

    public interface IEntityIndexKey<T> : IKeyEquatable<T>
        where T : unmanaged, IEntityIndexKey<T>
    {

    }
}