using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public static class QueryTestExtensions
    {
        public static List<(NB<T> buffer, IndexedIndices indices, ExclusiveGroupStruct group)> ToList<T>(this IndexQueryEnumerable<T> enumerable)
            where T : unmanaged, IEntityComponent
        {
            var list = new List<(NB<T> buffer, IndexedIndices indices, ExclusiveGroupStruct group)>();

            foreach (var value in enumerable)
            {
                var ((buffer, _), indices, group) = value;
                list.Add((buffer, indices, group));
            }

            return list;
        }

        public static List<(NB<T> buffer, IndexedIndices indices, ExclusiveGroupStruct group)> ToList<T>(this IndexQueryGroupsEnumerable<T> enumerable)
            where T : unmanaged, IEntityComponent
        {
            var list = new List<(NB<T> buffer, IndexedIndices indices, ExclusiveGroupStruct group)>();

            foreach (var value in enumerable)
            {
                var ((buffer, _), indices, group) = value;
                list.Add((buffer, indices, group));
            }

            return list;
        }
    }
}
