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

        internal IndexQuery(int indexerId, T key)
        {
            this.indexerId = indexerId;
            this.key = key;
        }

        private FasterDictionary<ExclusiveGroupStruct, SchemaContext.IndexerGroupData> GetGroupIndexDataList(SchemaContext context)
        {
            if (context.indexers[indexerId] == null)
                return null;

            var indexerData = (SchemaContext.IndexerData<T>)context.indexers[indexerId];
            indexerData.TryGetValue(key, out var result);
            return result;
        }

        public readonly ref partial struct FromGroup<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            public readonly Group<TDesc> group;

            public FromGroup(in Group<TDesc> group)
            {
                this.group = group;
            }
        }

        public readonly ref partial struct FromGroups<TDesc>
            where TDesc : IEntityDescriptor, new()
        {
            public readonly Groups<TDesc> groups;

            public FromGroups(in Groups<TDesc> groups)
            {
                this.groups = groups;
            }
        }

        public FromGroup<TDesc> From<TDesc>(in Group<TDesc> group)
            where TDesc : IEntityDescriptor, new()
        {
            return new FromGroup<TDesc>(group);
        }

        public FromGroups<TDesc> From<TDesc>(in Groups<TDesc> groups)
            where TDesc : IEntityDescriptor, new()
        {
            return new FromGroups<TDesc>(groups);
        }

    }
}