using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public static class QueryTestExtensions
    {
        public static List<((NB<T>, FilteredIndices), ExclusiveGroupStruct)> ToList<T>(this IndexQueryEnumerable<T> enumerable)
            where T : unmanaged, IEntityComponent
        {
            var list = new List<((NB<T>, FilteredIndices), ExclusiveGroupStruct)>();

            foreach (var value in enumerable)
            {
                var (tuple, group) = value;
                list.Add((tuple, group));
            }

            return list;
        }

        public static List<((NB<T>, FilteredIndices), ExclusiveGroupStruct)> ToList<T>(this IndexQueryGroupsEnumerable<T> enumerable)
            where T : unmanaged, IEntityComponent
        {
            var list = new List<((NB<T>, FilteredIndices), ExclusiveGroupStruct)>();

            foreach (var value in enumerable)
            {
                var (tuple, group) = value;
                list.Add((tuple, group));
            }

            return list;
        }
    }
}
