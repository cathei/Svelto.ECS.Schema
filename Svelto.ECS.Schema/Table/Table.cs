using System;
using System.Collections.Generic;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public abstract partial class TableBase
    {
        protected readonly ExclusiveGroupStruct _exclusiveGroup;

        public ref readonly ExclusiveGroupStruct ExclusiveGroup => ref _exclusiveGroup;

        internal TableBase()
        {
            _exclusiveGroup = new ExclusiveGroup();
        }

        public static implicit operator ExclusiveGroupStruct(in TableBase group) => group._exclusiveGroup;
    }

    public abstract class TableBase<TRow> : TableBase, IEntityTable<TRow>, IEntityTablesBuilder<TRow>
        where TRow : DescriptorRow<TRow>
    {
        internal TableBase() : base() { }

        public string Name { get; set; }

        IEnumerable<IEntityTable<TRow>> IEntityTablesBuilder<TRow>.Tables
        {
            get { yield return this; }
        }

        public override string ToString() => Name;
    }
}

namespace Svelto.ECS.Schema
{
    public sealed class Table<TRow> : TableBase<TRow>
        where TRow : DescriptorRow<TRow>
    { }

    public class Tables<TRow> : TablesBase<TRow>
        where TRow : DescriptorRow<TRow>
    {
        public Tables(int range) : base(GenerateTables(range), false) { }

        private static Table<TRow>[] GenerateTables(int range)
        {
            var tables = new Table<TRow>[range];

            for (int i = 0; i < range; ++i)
                tables[i] = new Table<TRow>();

            return tables;
        }
    }

    public sealed class Tables<TRow, TIndex> : Tables<TRow>
        where TRow : DescriptorRow<TRow>
    {
        internal readonly Func<TIndex, int> _mapper;

        internal Tables(int range, Func<TIndex, int> mapper) : base(range)
        {
            _mapper = mapper;
        }

        internal Tables(TIndex range, Func<TIndex, int> mapper) : base(mapper(range))
        {
            _mapper = mapper;
        }

        public IEntityTable<TRow> this[TIndex index] => _tables[_mapper(index)];
        public IEntityTable<TRow> Get(TIndex index) => _tables[_mapper(index)];
    }

}