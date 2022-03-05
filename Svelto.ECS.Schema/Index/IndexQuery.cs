using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IIndexQueryable<TRow, TKey>
        where TRow : IEntityRow
        where TKey : unmanaged, IKeyEquatable<TKey>
    {
        IndexQuery<TRow, TKey> Query(in TKey key);
    }

    public interface IIndexQuery
    {
        IndexerKeyData GetIndexerKeyData(IndexedDB indexedDB);
    }

    public readonly struct IndexQuery<TRow, TKey> : IIndexQuery
        where TRow : IEntityRow
        where TKey : unmanaged, IKeyEquatable<TKey>
    {
        private readonly int _indexerId;
        private readonly TKey _key;

        internal IndexQuery(int indexerId, in TKey key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        IndexerKeyData IIndexQuery.GetIndexerKeyData(IndexedDB indexedDB)
        {
            if (!indexedDB.indexers.ContainsKey(_indexerId))
                return default;

            var indexerData = (IndexerData<TKey>)indexedDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }
    }
}

