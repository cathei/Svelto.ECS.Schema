using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IIndexQueryable<TR, TK, TC>
        where TR : IIndexableRow<TK, TC>
        where TK : unmanaged
        where TC : unmanaged, IIndexableComponent<TK>
    {
        IndexQuery<TK, TC> Query(in TK key);
    }

    public interface IIndexQuery
    {
        IndexedKeyData GetIndexedKeyData(IndexedDB indexedDB);
    }

    // public interface IIndexQuery<TC> : IIndexQuery
    //     where TC : unmanaged, IEntityComponent
    // {
    //     NB<TC> GetComponents(IndexedDB indexedDB, in ExclusiveGroupStruct groupID);
    // }

    public readonly struct IndexQuery<TK, TC> : IIndexQuery
        where TK : unmanaged
        where TC : unmanaged, IIndexableComponent<TK>
    {
        private readonly int _indexerId;
        private readonly TK _key;

        internal IndexQuery(int indexerId, in TK key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        private IndexedKeyData GetIndexedKeyData(IndexedDB indexedDB)
        {
            if (!indexedDB.indexers.ContainsKey(_indexerId))
                return default;

            var indexerData = (IndexedData<TK>)indexedDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }

        IndexedKeyData IIndexQuery.GetIndexedKeyData(IndexedDB indexedDB)
            => GetIndexedKeyData(indexedDB);
    }
}

namespace Svelto.ECS.Schema
{
    // public readonly partial struct IndexQuery<TK, TC> : IEntityIndexQuery<TC>
    //     where TK : unmanaged
    //     where TC : unmanaged, IIndexedComponent<TK>
    // {
    //     private readonly int _indexerId;
    //     private readonly TK _key;

    //     internal IndexQuery(int indexerId, in TK key)
    //     {
    //         _indexerId = indexerId;
    //         _key = key;
    //     }

    //     private IndexedKeyData GetIndexedKeyData(IndexedDB indexedDB)
    //     {
    //         if (!indexedDB.indexers.ContainsKey(_indexerId))
    //             return default;

    //         var indexerData = (IndexedData<TK>)indexedDB.indexers[_indexerId];
    //         indexerData.TryGetValue(_key, out var result);
    //         return result;
    //     }

    //     IndexedKeyData IEntityIndexQuery.GetIndexedKeyData(IndexedDB indexedDB)
    //         => GetIndexedKeyData(indexedDB);

    //     // this is required because of limitation of filters
    //     // we cannot merge filters without EGID yet
    //     NB<TC> IEntityIndexQuery<TC>.GetComponents(IndexedDB indexedDB, in ExclusiveGroupStruct groupID)
    //     {
    //         return indexedDB.entitiesDB.QueryEntities<TC>(groupID).ToBuffer().buffer;
    //     }

    //     public void Set<T>(IndexedDB indexedDB, Memo<T> memo)
    //         where T : struct, IEntityComponent
    //     {
    //         memo.Set<IndexQuery<TK, TC>, TC>(indexedDB, this);
    //     }

    //     public void Union<T>(IndexedDB indexedDB, Memo<T> memo)
    //         where T : struct, IEntityComponent
    //     {
    //         memo.Union<IndexQuery<TK, TC>, TC>(indexedDB, this);
    //     }

    //     public void Intersect<T>(IndexedDB indexedDB, Memo<T> memo)
    //         where T : struct, IEntityComponent
    //     {
    //         memo.Intersect<IndexQuery<TK, TC>, TC>(indexedDB, this);
    //     }
    // }
}
