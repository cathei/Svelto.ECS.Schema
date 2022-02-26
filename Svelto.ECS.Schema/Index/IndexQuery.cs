using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public interface IEntityIndexQuery
        {
            IndexesDB.IndexerSetData GetGroupIndexDataList(IndexesDB indexesDB);
        }

        public interface IEntityIndexQuery<TK, TC> : IEntityIndexQuery
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            NB<TC> GetComponents(IndexesDB indexesDB, in ExclusiveGroupStruct groupID);
        }
    }

    public readonly partial struct IndexQuery<TK, TC> : IEntityIndexQuery<TK, TC>
        where TK : unmanaged, IKeyEquatable<TK>
        where TC : unmanaged, IIndexedComponent<TK>
    {
        private readonly int _indexerId;
        private readonly TK _key;

        internal IndexQuery(int indexerId, in TK key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        NB<TC> IEntityIndexQuery<TK, TC>.GetComponents(IndexesDB indexesDB, in ExclusiveGroupStruct groupID)
        {
            return indexesDB.entitiesDB.QueryEntities<TC>(groupID).ToBuffer().buffer;
        }

        private IndexesDB.IndexerSetData GetGroupIndexDataList(IndexesDB indexesDB)
        {
            if (!indexesDB.indexers.ContainsKey(_indexerId))
                return default;

            var indexerData = (IndexesDB.IndexerData<TK>)indexesDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }

        IndexesDB.IndexerSetData IEntityIndexQuery.GetGroupIndexDataList(IndexesDB indexesDB)
            => GetGroupIndexDataList(indexesDB);

        public void Set<T>(IndexesDB indexesDB, Memo<T> memo)
            where T : unmanaged, IEntityComponent
        {
            memo.Set<IndexQuery<TK, TC>, TK, TC>(indexesDB, this);
        }

        public void Union<T>(IndexesDB indexesDB, Memo<T> memo)
            where T : unmanaged, IEntityComponent
        {
            memo.Union<IndexQuery<TK, TC>, TK, TC>(indexesDB, this);
        }

        public void Intersect<T>(IndexesDB indexesDB, Memo<T> memo)
            where T : unmanaged, IEntityComponent
        {
            memo.Intersect<IndexQuery<TK, TC>, TK, TC>(indexesDB, this);
        }
    }
}