using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Internal;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public static class QueryTestExtensions
    {
        // public static List<(TResult set, FilteredIndices indices, IEntityTable<TRow> table)> ToList<TResult, TRow> (
        //         this FromRowSelectQuery<TRow, TResult> enumerable)
        //     where TResult : struct, IResultSet
        //     where TRow : class, IQueryableRow<TResult>
        // {
        //     var list = new List<(TResult, FilteredIndices, IEntityTable<TRow>)>();

        //     foreach (var value in enumerable)
        //     {
        //         list.Add((value.set, value.indices._indices, value.table));
        //     }

        //     return list;
        // }
    }
}
