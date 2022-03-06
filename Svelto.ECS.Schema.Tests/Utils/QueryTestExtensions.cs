using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public static class QueryTestExtensions
    {
        public static List<(NB<T1> buffer, FilteredIndices indices, IEntityTable<TR> table)> ToList<TR, T1> (
                this IndexQueryEnumerable<TR, T1> enumerable)
            where TR : class, ISelectorRow<T1>
            where T1 : unmanaged, IEntityComponent
        {
            var list = new List<(NB<T1> buffer, FilteredIndices indices, IEntityTable<TR> table)>();

            foreach (var value in enumerable)
            {
                var ((buffer, _), indices, group) = value;
                list.Add((buffer, indices._indices, group));
            }

            return list;
        }
    }
}
