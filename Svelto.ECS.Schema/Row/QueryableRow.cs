using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IQueryableRow<T> : IEntityRow
        where T : struct, IResultSet
    { }

    public ref struct QueryResult<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        public readonly TResult result;
        public readonly RangedIndices indices;
        public readonly IEntityTable<TRow> table;

        public QueryResult(in TResult result, IEntityTable<TRow> table)
        {
            this.result = result;
            this.indices = new RangedIndices((uint)result.count);
            this.table = table;
        }
    }

    public ref struct IndexedQueryResult<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        public readonly TResult result;
        public readonly IndexedIndices indices;
        public readonly IEntityTable<TRow> table;

        public IndexedQueryResult(in TResult result, in IndexedIndices indices, IEntityTable<TRow> table)
        {
            this.result = result;
            this.indices = indices;
            this.table = table;
        }
    }
}