using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Hybrid;
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

        IEnumerable<IEntityTable<TRow>> IEntityTablesBuilder<TRow>.Tables
        {
            get { yield return this; }
        }
    }
}