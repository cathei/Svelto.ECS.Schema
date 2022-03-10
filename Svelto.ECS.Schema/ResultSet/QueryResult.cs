using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public ref struct QueryResult<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        public readonly TResult set;
        public readonly RangedIndices indices => new RangedIndices((uint)set.count);
        public readonly IEntityTable<TRow> table;

        public QueryResult(in TResult resultSet, IEntityTable<TRow> table)
        {
            this.set = resultSet;
            this.table = table;
        }
    }

    public ref struct IndexedQueryResult<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        public readonly TResult set;
        public readonly IndexedIndices indices;
        public readonly IEntityTable<TRow> table;

        public IndexedQueryResult(in TResult resultSet, in IndexedIndices indices, IEntityTable<TRow> table)
        {
            this.set = resultSet;
            this.indices = indices;
            this.table = table;
        }
    }
}