using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IIndexQueryable<TRow, TComponent>
        where TRow : class, IEntityRow
        where TComponent : unmanaged, IEntityComponent
    {
        public int IndexerID { get; }
    }

    public readonly ref struct IndexQuery
    {
        internal readonly IndexerKeyData _keyData;

        internal IndexQuery(IndexerKeyData keyData)
        {
            _keyData = keyData;
        }
    }

    public interface IIndexQuery
    {
        IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB);
    }

    public readonly struct IndexQuery<TRow, TKey> : IIndexQuery
        where TRow : class, IEntityRow
        where TKey : unmanaged, IEquatable<TKey>
    {
        private readonly int _indexerId;
        private readonly TKey _key;

        internal IndexQuery(int indexerId, in TKey key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        internal IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB)
        {
            if (!indexedDB.indexers.ContainsKey(_indexerId))
                return default;

            var indexerData = (IndexerData<TKey>)indexedDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }

        IndexerKeyData IIndexQuery.GetIndexerKeyData(IndexedDB indexedDB)
            => GetIndexerKeyData(indexedDB);
    }
}

