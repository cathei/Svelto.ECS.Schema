using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly partial struct IndexQuery<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        internal readonly int indexerId;
        internal readonly T key;

        private static readonly FilteredIndices EmptyFilteredIndices = new FilteredIndices();

        internal IndexQuery(int indexerId, in T key)
        {
            this.indexerId = indexerId;
            this.key = key;
        }

        private FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(SchemaContext context)
        {
            if (!context.indexers.ContainsKey(indexerId))
                return null;

            var indexerData = (SchemaContext.IndexerData<T>)context.indexers[indexerId];
            indexerData.TryGetValue(key, out var result);
            return result;
        }

        public readonly partial struct FromGroup<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            private readonly IndexQuery<T> query;
            private readonly ExclusiveGroupStruct group;

            public FromGroup(in IndexQuery<T> query, in Group<TDesc> group)
            {
                this.query = query;
                this.group = group;
            }
        }

        public readonly partial struct FromGroups<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            private readonly IndexQuery<T> query;
            private readonly FasterList<ExclusiveGroupStruct> groups;

            public FromGroups(in IndexQuery<T> query, in Groups<TDesc> groups)
            {
                this.query = query;
                this.groups = groups;
            }
        }

        public FromGroup<TDesc> From<TDesc>(in Group<TDesc> group)
            where TDesc : IEntityDescriptor, new()
        {
            return new FromGroup<TDesc>(this, group);
        }

        public FromGroups<TDesc> From<TDesc>(in Groups<TDesc> groups)
            where TDesc : IEntityDescriptor, new()
        {
            return new FromGroups<TDesc>(this, groups);
        }
    }
}