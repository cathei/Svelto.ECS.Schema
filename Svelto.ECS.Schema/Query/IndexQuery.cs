using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly ref partial struct IndexQuery<TKey>
        where TKey : unmanaged, IEntityIndexKey<TKey>
    {
        private readonly int _indexerId;
        private readonly TKey _key;

        internal IndexQuery(int indexerId, in TKey key)
        {
            _indexerId = indexerId;
            _key = key;
        }

        internal FasterDictionary<ExclusiveGroupStruct, IndexesDB.IndexerGroupData> GetGroupIndexDataList(IndexesDB indexesDB)
        {
            if (!indexesDB.indexers.ContainsKey(_indexerId))
                return null;

            var indexerData = (IndexesDB.IndexerData<TKey>)indexesDB.indexers[_indexerId];
            indexerData.TryGetValue(_key, out var result);
            return result;
        }

        public IndexGroupQuery<TKey, TDesc> From<TDesc>(in Group<TDesc> group)
            where TDesc : IEntityDescriptor, new()
        {
            return new IndexGroupQuery<TKey, TDesc>(this, group);
        }

        public IndexGroupsQuery<TKey, TDesc> From<TDesc>(in Groups<TDesc> groups)
            where TDesc : IEntityDescriptor, new()
        {
            return new IndexGroupsQuery<TKey, TDesc>(this, groups);
        }
    }

    public readonly ref partial struct IndexGroupQuery<TKey, TDesc>
        where TKey : unmanaged, IEntityIndexKey<TKey>
        where TDesc : IEntityDescriptor, new()
    {
        private readonly IndexQuery<TKey> _query;
        private readonly Group<TDesc> _group;

        public IndexGroupQuery(in IndexQuery<TKey> query, in Group<TDesc> group)
        {
            _query = query;
            _group = group;
        }
    }

    public readonly ref partial struct IndexGroupsQuery<TKey, TDesc>
        where TKey : unmanaged, IEntityIndexKey<TKey>
        where TDesc : IEntityDescriptor, new()
    {
        private readonly IndexQuery<TKey> _query;
        private readonly Groups<TDesc> _groups;

        public IndexGroupsQuery(in IndexQuery<TKey> query, in Groups<TDesc> groups)
        {
            _query = query;
            _groups = groups;
        }
    }
}