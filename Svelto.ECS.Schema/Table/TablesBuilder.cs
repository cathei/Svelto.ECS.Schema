using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IEntityTablesBuilder<out TRow>
        where TRow : class, IEntityRow
    {
        IEnumerable<IEntityTable<TRow>> Tables { get; }
    }
}

namespace Svelto.ECS.Schema
{
    public readonly struct TablesBuilder<TRow> : IEntityTablesBuilder<TRow>
        where TRow : class, IEntityRow
    {
        internal readonly IEnumerable<IEntityTable<TRow>> _tables;

        public IEnumerable<IEntityTable<TRow>> Tables => _tables;

        public TablesBuilder(IEnumerable<IEntityTable<TRow>> items) => _tables = items;

        public CombinedTables<TRow> Build() => new CombinedTables<TRow>(_tables);

        public static implicit operator CombinedTables<TRow>(in TablesBuilder<TRow> builder) => builder.Build();
    }

    public static class TablesBuilderExtensions
    {
        public static TablesBuilder<TRow> Append<TRow>(
                this IEntityTablesBuilder<TRow> a, IEntityTablesBuilder<TRow> b)
            where TRow : class, IEntityRow
        {
            return new TablesBuilder<TRow>(a.Tables.Concat(b.Tables));
        }
    }
}