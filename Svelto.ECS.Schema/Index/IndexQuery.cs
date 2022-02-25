using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public interface IIndexQuery
        {
            IndexesDB.IndexerSetData GetGroupIndexDataList(IndexesDB indexesDB);
        }
    }

    public readonly partial struct IndexQuery<TKey> : IIndexQuery
        where TKey : unmanaged, IKeyEquatable<TKey>
    {
        private readonly int _indexerId;
        private readonly TKey _key;

        internal IndexQuery(int indexerId, in TKey key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        IndexesDB.IndexerSetData IIndexQuery.GetGroupIndexDataList(IndexesDB indexesDB)
        {
            if (!indexesDB.indexers.ContainsKey(_indexerId))
                return default;

            var indexerData = (IndexesDB.IndexerData<TKey>)indexesDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }
    }
}