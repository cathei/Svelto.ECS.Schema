using System;
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

        public interface IEntityIndexQuery<TC> : IEntityIndexQuery
            where TC : unmanaged, IEntityComponent
        {
            NB<TC> GetComponents(IndexesDB indexesDB, in ExclusiveGroupStruct groupID);
        }
    }

    public readonly partial struct IndexQuery<TK, TC> : IEntityIndexQuery<TC>
        where TK : unmanaged
        where TC : unmanaged, IIndexedComponent<TK>
    {
        private readonly int _indexerId;
        private readonly TK _key;

        internal IndexQuery(int indexerId, in TK key)
        {
            _indexerId = indexerId;
            _key = key;
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

        // this is required because of limitation of filters
        // we cannot merge filters without EGID yet
        NB<TC> IEntityIndexQuery<TC>.GetComponents(IndexesDB indexesDB, in ExclusiveGroupStruct groupID)
        {
            return indexesDB.entitiesDB.QueryEntities<TC>(groupID).ToBuffer().buffer;
        }

        public void Set<T>(IndexesDB indexesDB, Memo<T> memo)
            where T : struct, IEntityComponent
        {
            memo.Set<IndexQuery<TK, TC>, TC>(indexesDB, this);
        }

        public void Union<T>(IndexesDB indexesDB, Memo<T> memo)
            where T : struct, IEntityComponent
        {
            memo.Union<IndexQuery<TK, TC>, TC>(indexesDB, this);
        }

        public void Intersect<T>(IndexesDB indexesDB, Memo<T> memo)
            where T : struct, IEntityComponent
        {
            memo.Intersect<IndexQuery<TK, TC>, TC>(indexesDB, this);
        }
    }
}
