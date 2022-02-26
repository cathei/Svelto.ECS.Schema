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
    public readonly ref partial struct IndexGroupQuery<TQuery, TDesc>
        where TQuery : IEntityIndexQuery
        where TDesc : IEntityDescriptor, new()
    {
        private readonly TQuery _query;
        private readonly Table<TDesc> _group;

        public IndexGroupQuery(in TQuery query, Table<TDesc> table)
        {
            _query = query;
            _group = table;
        }
    }

    public readonly ref partial struct IndexGroupsQuery<TQuery, TDesc>
        where TQuery : IEntityIndexQuery
        where TDesc : IEntityDescriptor, new()
    {
        private readonly TQuery _query;
        private readonly Tables<TDesc> _groups;

        public IndexGroupsQuery(in TQuery query, in Tables<TDesc> tables)
        {
            _query = query;
            _groups = tables;
        }
    }

    public static partial class IndexQueryExtensions
    {
        public static IndexGroupQuery<TQuery, TDesc> From<TQuery, TDesc>(this TQuery query, Table<TDesc> table)
            where TQuery : IEntityIndexQuery
            where TDesc : IEntityDescriptor, new()
        {
            return new IndexGroupQuery<TQuery, TDesc>(query, table);
        }

        public static IndexGroupsQuery<TQuery, TDesc> From<TQuery, TDesc>(this TQuery query, Tables<TDesc> tables)
            where TQuery : IEntityIndexQuery
            where TDesc : IEntityDescriptor, new()
        {
            return new IndexGroupsQuery<TQuery, TDesc>(query, tables);
        }
    }
}