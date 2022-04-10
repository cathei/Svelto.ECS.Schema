using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IWhereQuery
    {
        internal void Apply(ResultSetQueryConfig config);
    }

    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IWhereQuery<in TRow> : IWhereQuery
        where TRow : class, IEntityRow
    { }

    public interface IWhereQueryable
    {
        internal void Apply<TKey>(ResultSetQueryConfig config, TKey key)
            where TKey : unmanaged, IEquatable<TKey>;
    }

    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IWhereQueryable<in TRow, TComponent> : IWhereQueryable
        where TRow : class, IKeyedRow<TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }

    public struct WhereQuery<TRow, TKey> : IWhereQuery<TRow>
        where TRow : class, IEntityRow
        where TKey : unmanaged, IEquatable<TKey>
    {
        private readonly IWhereQueryable _queryable;
        private readonly TKey _key;

        internal WhereQuery(IWhereQueryable queryable, TKey key)
        {
            _queryable = queryable;
            _key = key;
        }

        void IWhereQuery.Apply(ResultSetQueryConfig config)
        {
            _queryable.Apply(config, _key);
        }
    }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// this has to be extensions because WhereQueryable does not know what is TKey
    /// </summary>
    public static class WhereQueryExtensions
    {
        public static WhereQuery<TRow, TKey> Is<TRow, TComponent, TKey>(
                this IWhereQueryable<TRow, TComponent> queryable, TKey key)
            where TRow : class, IKeyedRow<TComponent>
            where TComponent : unmanaged, IKeyComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            return new(queryable, key);
        }
    }

    public static class WhereQueryEnumExtensions
    {
        public static WhereQuery<TRow, EnumKey<TKey>> Is<TRow, TComponent, TKey>(
                this IWhereQueryable<TRow, TComponent> queryable, TKey key)
            where TRow : class, IKeyedRow<TComponent>
            where TComponent : unmanaged, IKeyComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            return new(queryable, new EnumKey<TKey>(key));
        }
    }
}
