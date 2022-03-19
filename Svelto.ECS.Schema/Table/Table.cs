using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // public abstract partial class TableBase
    // {
    //     protected readonly ExclusiveGroupStruct _exclusiveGroup;

    //     public ref readonly ExclusiveGroupStruct ExclusiveGroup => ref _exclusiveGroup;

    //     internal TableBase()
    //     {
    //         _exclusiveGroup = new ExclusiveGroup();
    //     }

    //     public static implicit operator ExclusiveGroupStruct(in TableBase group) => group._exclusiveGroup;
    // }

    public abstract class Table : IEntityTable
    {
        public string Name { get; internal set; }

        public override string ToString() => Name;

        internal ExclusiveGroup group;
        internal int groupRange;

        public ref readonly ExclusiveGroup Group => ref group;
        public int GroupRange => groupRange;

        internal struct PrimaryKeyInfo
        {
            internal int id;
            internal ushort possibleKeyCount;
        }

        internal FasterList<PrimaryKeyInfo> primaryKeys = new FasterList<PrimaryKeyInfo>();

        internal Table() { }

        protected abstract EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
        protected abstract void Remove(IEntityFunctions functions, uint entityID, in ExclusiveGroupStruct groupID);

        EntityInitializer IEntityTable.Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors)
            => Build(factory, entityID, implementors);

        /// <summary>
        /// Schema extensions only support remove from untyped table for now
        /// Because it is not possible to determine if it is safe to swap to other table
        /// After Primary Key implemented it should be used instead of swap.
        /// </summary>
        void IEntityTable.Remove(IEntityFunctions functions, uint entityID, in ExclusiveGroupStruct groupID)
            => Remove(functions, entityID, groupID);
    }
}

namespace Svelto.ECS.Schema
{
    public sealed class Table<TRow> : Table, IEntityTable<TRow>, IEntityTables<TRow>
        where TRow : DescriptorRow<TRow>
    {
        private delegate void InitializerDelegate(in EntityInitializer initializer);

        private FasterList<InitializerDelegate> _defaultInitializerActions;

        public void AddPrimaryKey<TPrimaryKey>(TPrimaryKey primaryKey)
            where TPrimaryKey : IPrimaryKeyProvider<TRow>
        {
            primaryKeys.Add(new PrimaryKeyInfo
            {
                id = primaryKey.PrimaryKeyID,
                possibleKeyCount = primaryKey.PossibleKeyCount
            });
        }

        public void SetDefault<T>(T initialValue)
            where T : struct, IEntityComponent
        {
            _defaultInitializerActions ??= new FasterList<InitializerDelegate>();
            _defaultInitializerActions.Add((in EntityInitializer builder) => builder.Init(initialValue));
        }

        protected override EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors)
        {
            var builder = factory.BuildEntity<DescriptorRow<TRow>.Descriptor>(entityID, group, implementors);

            if (_defaultInitializerActions != null)
            {
                foreach (var action in _defaultInitializerActions)
                    action(builder);
            }

            return builder;
        }

        protected override void Remove(IEntityFunctions functions, uint entityID, in ExclusiveGroupStruct groupID)
        {
            functions.RemoveEntity<DescriptorRow<TRow>.Descriptor>(entityID, groupID);
        }

        int IEntityTables.Range => 1;

        IEntityTable IEntityTables.GetTable(int index) => this;

        IEntityTable<TRow> IEntityTables<TRow>.GetTable(int index) => this;

        IEnumerable<IEntityTable<TRow>> IEntityTablesBuilder<TRow>.Tables
        {
            get { yield return this; }
        }

        LocalFasterReadOnlyList<PrimaryKeyInfo> IEntityTable<TRow>.PrimaryKeys => primaryKeys;
    }
}