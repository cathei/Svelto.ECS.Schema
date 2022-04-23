using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed class CombinedTables<TRow> : IEntityTables<TRow>
        where TRow : class, IEntityRow
    {
        internal readonly IEntityTable<TRow>[] _tables;
        internal FasterList<ExclusiveGroupStruct> _groups;

        public int Range => _tables.Length;

        internal CombinedTables(IEntityTable<TRow>[] tables)
        {
            _tables = tables;
        }

        internal CombinedTables(IEnumerable<IEntityTable<TRow>> tables) : this(tables.ToArray()) { }
        internal CombinedTables(FasterList<IEntityTable<TRow>> tables) : this(tables.ToArray()) { }

        private LocalFasterReadOnlyList<ExclusiveGroupStruct> Build()
        {
            if (_groups != null)
                return _groups;

            _groups = new FasterList<ExclusiveGroupStruct>(_tables.SelectMany(
                x => Enumerable.Range(0, x.GroupRange).Select(i => x.Group + (uint)i)
            ).ToArray());

            return _groups;
        }

        public IEntityTable<TRow> this[int index] => _tables[index];
        public IEntityTable<TRow> Get(int index) => _tables[index];

        public IEntityTable<TRow> this[uint index] => _tables[index];
        public IEntityTable<TRow> Get(uint index) => _tables[index];

        public static implicit operator TablesBuilder<TRow>(CombinedTables<TRow> rangedTable)
            => new TablesBuilder<TRow>(rangedTable._tables);

        IEnumerable<IEntityTable<TRow>> IEntityTablesBuilder<TRow>.Tables => _tables;

        public LocalFasterReadOnlyList<ExclusiveGroupStruct> ExclusiveGroups => Build();

        ITableDefinition ITablesDefinition.GetTable(int index) => _tables[index];
        IEntityTable<TRow> IEntityTables<TRow>.GetTable(int index) => _tables[index];
    }
}