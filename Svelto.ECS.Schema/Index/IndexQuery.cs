using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public readonly ref partial struct IndexQuery<TKey>
        where TKey : unmanaged, IKeyEquatable<TKey>
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
            return result.groups;
        }

        public IndexGroupQuery<TKey, TDesc> From<TDesc>(Table<TDesc> table)
            where TDesc : IEntityDescriptor, new()
        {
            return new IndexGroupQuery<TKey, TDesc>(this, table);
        }

        public IndexGroupsQuery<TKey, TDesc> From<TDesc>(in Tables<TDesc> tables)
            where TDesc : IEntityDescriptor, new()
        {
            return new IndexGroupsQuery<TKey, TDesc>(this, tables);
        }
    }

    public readonly ref partial struct IndexGroupQuery<TKey, TDesc>
        where TKey : unmanaged, IKeyEquatable<TKey>
        where TDesc : IEntityDescriptor, new()
    {
        private readonly IndexQuery<TKey> _query;
        private readonly Table<TDesc> _group;

        public IndexGroupQuery(in IndexQuery<TKey> query, Table<TDesc> table)
        {
            _query = query;
            _group = table;
        }
    }

    public readonly ref partial struct IndexGroupsQuery<TKey, TDesc>
        where TKey : unmanaged, IKeyEquatable<TKey>
        where TDesc : IEntityDescriptor, new()
    {
        private readonly IndexQuery<TKey> _query;
        private readonly Tables<TDesc> _groups;

        public IndexGroupsQuery(in IndexQuery<TKey> query, in Tables<TDesc> tables)
        {
            _query = query;
            _groups = tables;
        }
    }
}