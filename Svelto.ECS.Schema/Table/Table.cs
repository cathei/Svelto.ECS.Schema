using System;
using System.Collections.Generic;
using Svelto.DataStructures;
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

        private delegate void InitializerDelegate(in EntityInitializer initializer);

        private FasterList<InitializerDelegate> _defaultInitializerActions;

        public void SetDefault<T>(T initialValue)
            where T : struct, IEntityComponent
        {
            _defaultInitializerActions ??= new FasterList<InitializerDelegate>();
            _defaultInitializerActions.Add((in EntityInitializer builder) => builder.Init(initialValue));
        }

        EntityInitializer IEntityTable<TRow>.Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors)
        {
            var builder = factory.BuildEntity<DescriptorRow<TRow>.Descriptor>(entityID, _exclusiveGroup, implementors);

            if (_defaultInitializerActions != null)
            {
                foreach (var action in _defaultInitializerActions)
                    action(builder);
            }

            return builder;
        }

        /// <summary>
        /// Schema extensions only support remove from untyped table for now
        /// Because it is not possible to determine if it is safe to swap to other table
        /// After Primary Key implemented it should be used instead of swap.
        /// </summary>
        void IEntityTable<TRow>.Remove(IEntityFunctions functions, uint entityID)
        {
            functions.RemoveEntity<DescriptorRow<TRow>.Descriptor>(entityID, _exclusiveGroup);
        }
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