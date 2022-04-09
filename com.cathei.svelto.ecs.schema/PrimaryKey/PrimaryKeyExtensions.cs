using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class PrimaryKeyExtensions
    {
        public static void SetPossibleKeys<TComponent, TKey>(this PrimaryKey<TComponent> primaryKey, params TKey[] possibleKeys)
            where TComponent : unmanaged, IKeyComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            FasterDictionary<TKey, int> dict = new FasterDictionary<TKey, int>((uint)possibleKeys.Length);

            for (int i = 0; i < possibleKeys.Length; ++i)
                dict.Add(possibleKeys[i], i);

            primaryKey._componentToIndex = component => dict[component.key];
            primaryKey._keyToIndex = new Func<TKey, int>(key => dict[key]);

            primaryKey.PossibleKeyCount = (ushort)dict.count;
        }
    }

    public static class PrimaryKeyEnumExtensions
    {
        public static void SetPossibleKeys<TComponent, TKey>(this PrimaryKey<TComponent> primaryKey, params TKey[] possibleKeys)
            where TComponent : unmanaged, IKeyComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            FasterDictionary<EnumKey<TKey>, int> dict = new FasterDictionary<EnumKey<TKey>, int>((uint)possibleKeys.Length);

            for (int i = 0; i < possibleKeys.Length; ++i)
                dict.Add(possibleKeys[i], i);

            primaryKey._componentToIndex = component => dict[component.key];
            primaryKey._keyToIndex = new Func<EnumKey<TKey>, int>(key => dict[key]);

            primaryKey.PossibleKeyCount = (ushort)dict.count;
        }
    }
}
