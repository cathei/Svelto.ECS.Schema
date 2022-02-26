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
    public readonly ref partial struct IndexGroupQuery<TQuery>
        where TQuery : IEntityIndexQuery
    {
        private readonly TQuery _query;
        private readonly ExclusiveGroupStruct _group;

        public IndexGroupQuery(in TQuery query, in ExclusiveGroupStruct group)
        {
            _query = query;
            _group = group;
        }

        public IndexedIndices Indices(IndexesDB indexesDB)
        {
            var setData = _query.GetGroupIndexDataList(indexesDB);

            if (setData.groups == null || !setData.groups.TryGetValue(_group, out var groupData))
                return default;

            return new IndexedIndices(groupData.filter.filteredIndices);
        }
    }

    public readonly ref partial struct IndexGroupsQuery<TQuery>
        where TQuery : IEntityIndexQuery
    {
        private readonly TQuery _query;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public IndexGroupsQuery(in TQuery query, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _query = query;
            _groups = groups;
        }
    }

    public static partial class IndexQueryExtensions
    {
        public static IndexGroupQuery<TQuery> From<TQuery>(this TQuery query, in ExclusiveGroupStruct group)
            where TQuery : IEntityIndexQuery
        {
            return new IndexGroupQuery<TQuery>(query, group);
        }

        public static IndexGroupsQuery<TQuery> From<TQuery>(this TQuery query,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
            where TQuery : IEntityIndexQuery
        {
            return new IndexGroupsQuery<TQuery>(query, groups);
        }
    }
}