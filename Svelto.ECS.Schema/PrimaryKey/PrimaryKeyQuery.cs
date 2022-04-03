using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public struct PrimaryKeyQuery<TRow> : IIndexQuery<TRow>
        where TRow : class, IEntityRow
    {
        internal int _primaryKeyID;
        internal int _primaryKeyValue;

        internal PrimaryKeyQuery(int pkID, int pkValue)
        {
            _primaryKeyID = pkID;
            _primaryKeyValue = pkValue;
        }

        void IIndexQuery.Apply(ResultSetQueryConfig config)
        {
            config.pkToValue.Add(_primaryKeyID, _primaryKeyValue);
        }
    }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// this has to be extensions because PrimaryKeyQueryable does not know what is TKey
    /// </summary>
    public static class PrimaryKeyQueryExtensions
    {
        public static PrimaryKeyQuery<TRow> Is<TRow, TComponent, TKey>(
                this IPrimaryKeyQueryable<TRow, TComponent> queryable, TKey key)
            where TRow : class, IPrimaryKeyRow<TComponent>
            where TComponent : unmanaged, IKeyComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            var keyToIndex = (Func<TKey, int>)queryable.KeyToIndex;
            return new PrimaryKeyQuery<TRow>(queryable.PrimaryKeyID, keyToIndex(key));
        }
    }

    public static class PrimaryKeyQueryEnumExtensions
    {
        public static PrimaryKeyQuery<TRow> Is<TRow, TComponent, TKey>(
                this IPrimaryKeyQueryable<TRow, TComponent> queryable, TKey key)
            where TRow : class, IPrimaryKeyRow<TComponent>
            where TComponent : unmanaged, IKeyComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            var keyToIndex = (Func<EnumKey<TKey>, int>)queryable.KeyToIndex;
            return new PrimaryKeyQuery<TRow>(queryable.PrimaryKeyID, keyToIndex(key));
        }
    }
}