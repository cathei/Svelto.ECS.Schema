using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly ref partial struct IndexQuery<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        internal readonly int indexerId;
        internal readonly T key;

        internal IndexQuery(int indexerId, in T key)
        {
            this.indexerId = indexerId;
            this.key = key;
        }

        internal FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(SchemaContext context)
        {
            if (!context.indexers.ContainsKey(indexerId))
                return null;

            var indexerData = (SchemaContext.IndexerData<T>)context.indexers[indexerId];
            indexerData.TryGetValue(key, out var result);
            return result;
        }

        public readonly ref partial struct FromGroupAccessor<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            private readonly IndexQuery<T> _query;
            private readonly ExclusiveGroupStruct _group;

            public FromGroupAccessor(in IndexQuery<T> query, in Group<TDesc> group)
            {
                _query = query;
                _group = group;
            }
        }

        public readonly ref partial struct FromGroupsAccessor<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            private readonly IndexQuery<T> _query;
            private readonly FasterList<ExclusiveGroupStruct> _groups;

            public FromGroupsAccessor(in IndexQuery<T> query, in Groups<TDesc> groups)
            {
                _query = query;
                _groups = groups;
            }
        }

        public FromGroupAccessor<TDesc> From<TDesc>(in Group<TDesc> group)
            where TDesc : IEntityDescriptor, new()
        {
            return new FromGroupAccessor<TDesc>(this, group);
        }

        public FromGroupsAccessor<TDesc> From<TDesc>(in Groups<TDesc> groups)
            where TDesc : IEntityDescriptor, new()
        {
            return new FromGroupsAccessor<TDesc>(this, groups);
        }
    }
}