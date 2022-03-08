using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // /// <summary>
    // /// contravariance (in) for TRow
    // /// </summary>
    public interface IIndexQueryable<in TRow, TComponent>
        where TRow : class, IEntityRow
        where TComponent : unmanaged, IEntityComponent
    {
        public int IndexerID { get; }
    }

    public interface IIndexQuery
    {
        internal IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB);
    }

    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IIndexQuery<in TRow> : IIndexQuery { }

    public readonly struct IndexQuery<TRow, TKey> : IIndexQuery<TRow>
        where TKey : unmanaged, IEquatable<TKey>
    {
        internal readonly int _indexerID;
        internal readonly TKey _key;

        // internal readonly IndexerKeyData _keyData;

        internal IndexQuery(int indexerID, TKey key)
        {
            _indexerID = indexerID;
            _key = key;
        }

        IndexerKeyData IIndexQuery.GetIndexerKeyData(IndexedDB indexedDB)
        {
            if (!indexedDB.indexers.ContainsKey(_indexerID))
                return default;

            var indexerData = (IndexerData<TKey>)indexedDB.indexers[_indexerID];
            indexerData.TryGetValue(_key, out var result);
            return result;
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
            where TComponent : unmanaged, IIndexableComponent<TKey>
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
            where TComponent : unmanaged, IIndexableComponent<EnumKey<TKey>>
            where TKey : unmanaged, Enum
        {
            return new IndexQuery<TRow, EnumKey<TKey>>(queryable.IndexerID, key);
        }
    }
}

