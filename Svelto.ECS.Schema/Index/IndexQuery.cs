using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        public interface IIndexQuery
        {
            IndexesDB.IndexerSetData GetGroupIndexDataList(IndexesDB indexesDB);
        }

        public interface IIndexQuery<TK, TC> : IIndexQuery
            where TK : unmanaged, IKeyEquatable<TK>
            where TC : unmanaged, IIndexedComponent<TK>
        {
            NB<TC> GetComponents(IndexesDB indexesDB, in ExclusiveGroupStruct groupID);
        }
    }

    public readonly partial struct IndexQuery<TK, TC> : IIndexQuery<TK, TC>
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

        NB<TC> IIndexQuery<TK, TC>.GetComponents(IndexesDB indexesDB, in ExclusiveGroupStruct groupID)
        {
            return indexesDB.entitiesDB.QueryEntities<TC>(groupID).ToBuffer().buffer;
        }

        IndexesDB.IndexerSetData IIndexQuery.GetGroupIndexDataList(IndexesDB indexesDB)
        {
            if (!indexesDB.indexers.ContainsKey(_indexerId))
                return default;

            var indexerData = (IndexesDB.IndexerData<TK>)indexesDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }

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