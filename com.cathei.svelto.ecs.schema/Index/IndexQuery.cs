using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly struct IndexQuery<TRow, TKey> : IWhereQuery<TRow>
        where TRow : class, IEntityRow
        where TKey : unmanaged, IEquatable<TKey>
    {
        internal readonly FilterContextID _indexerID;
        internal readonly TKey _key;

        internal IndexQuery(FilterContextID indexerID, TKey key)
        {
            _indexerID = indexerID;
            _key = key;
        }

        void IWhereQuery.Apply(ResultSetQueryConfig config)
        {
            config.filters.Add(GetFilter(config.indexedDB));
        }

        internal ref EntityFilterCollection GetFilter(IndexedDB indexedDB)
        {
            return ref indexedDB.GetFilter(_indexerID, _key);
        }
    }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// this has to be extensions because IndexQueryable does not know what is TKey
    /// </summary>
    public static class IndexQueryExtensions
    {
        public static IndexQuery<TRow, TKey> Is<TRow, TComponent, TKey>(
                this IIndexQueryable<TRow, TComponent> queryable, TKey key)
            where TRow : class, IIndexableRow<TComponent>
            where TComponent : unmanaged, IKeyComponent<TKey>
            where TKey : unmanaged, IEquatable<TKey>
        {
            return new IndexQuery<TRow, TKey>(queryable.IndexerID, key);
        }
    }

    public static class IndexQueryEnumExtensions
    {
        public static IndexQuery<TRow, EnumKey<TKey>> Is<TRow, TComponent, TKey>(
                this IIndexQueryable<TRow, TComponent> queryable, TKey key)
            where TRow : class, IIndexableRow<TComponent>
            where TComponent : unmanaged, IKeyComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            return new IndexQuery<TRow, EnumKey<TKey>>(queryable.IndexerID, key);
        }
    }
}

