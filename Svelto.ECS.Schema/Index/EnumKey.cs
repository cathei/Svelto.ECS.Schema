using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// C# generic enums causes boxing so much we need wrapper
    /// </summary>
    public readonly struct EnumKey<TEnum> : IEquatable<EnumKey<TEnum>>, IEquatable<TEnum>
        where TEnum : Enum
    {
        private readonly TEnum _value;

        private static readonly EqualityComparer<TEnum> Comparer = EqualityComparer<TEnum>.Default;

        public EnumKey(TEnum value)
        {
            _value = value;
        }

        public static implicit operator TEnum(EnumKey<TEnum> key) => key._value;
        public static implicit operator EnumKey<TEnum>(TEnum value) => new EnumKey<TEnum>(value);

        public bool Equals(TEnum other)
        {
            return Comparer.Equals(_value, other);
        }

        public bool Equals(EnumKey<TEnum> other)
        {
            return Comparer.Equals(_value, other._value);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(_value);
        }

        public override bool Equals(object obj)
        {
            return obj is EnumKey<TEnum> other && Equals(other);
        }
    }
}
