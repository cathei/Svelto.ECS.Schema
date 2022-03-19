using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    /***

Primary Key Component is concept of Index that split Tables
Any Index Component or State Machine Component can be used as Primary Key Component as well. 

Since the groups must stay statically - primary key component have to give you the possible range.

Primary key also have to support partial Query.

    ***/

    // contravariance (in) for TRow, for type check
    public interface IPrimaryKeyProvider<in TRow>
        where TRow : class, IEntityRow
    {
        public int PrimaryKeyID { get; }
        public ushort PossibleKeyCount { get; }
    }

    // contravariance (in) for TRow, for type check
    public interface IPrimaryKeyQueryable<in TRow, TComponent>
    {
        public int PrimaryKeyID { get; }
        public Delegate KeyToIndex { get; }
    }

    /// <summary>
    /// Default primary key when no primary key specified for the table
    /// </summary>
    internal class NullPrimaryKey : IPrimaryKeyProvider<IEntityRow>
    {
        public int PrimaryKeyID => 0;
        public int PossibleKeyCount => 1;
    }

    internal static class GlobalPrimaryKeyCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }
}

namespace Svelto.ECS.Schema
{
    public class PrimaryKey<TComponent> :
            IPrimaryKeyProvider<IPrimaryKeyRow<TComponent>>,
            IPrimaryKeyQueryable<IPrimaryKeyRow<TComponent>, TComponent>
        where TComponent : unmanaged, IPrimaryKeyComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _primaryKeyID = GlobalPrimaryKeyCount.Generate();

        internal Func<TComponent, int> _componentToIndex;
        internal Delegate _keyToIndex;

        public int PrimaryKeyID => _primaryKeyID;
        public ushort PossibleKeyCount { get; internal set; }

        Delegate IPrimaryKeyQueryable<IPrimaryKeyRow<TComponent>, TComponent>.KeyToIndex => _keyToIndex;
    }

    public static class PrimaryKeyExtensions
    {
        public static void SetPossibleKeys<TComponent, TKey>(this PrimaryKey<TComponent> primaryKey, TKey[] possibleKeys)
            where TComponent : unmanaged, IPrimaryKeyComponent<TKey>
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
        public static void SetPossibleKeys<TComponent, TKey>(this PrimaryKey<TComponent> primaryKey, TKey[] possibleKeys)
            where TComponent : unmanaged, IPrimaryKeyComponent<EnumKey<TKey>>
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
