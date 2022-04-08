using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public abstract class Table : IEntityTable
    {
        public string Name { get; internal set; }

        public override string ToString() => Name;

        internal ExclusiveGroup group;
        internal int groupRange;

        public ref readonly ExclusiveGroup Group => ref group;
        public int GroupRange => groupRange;

        internal FasterDictionary<int, IEntityPrimaryKey> primaryKeys = new();
        FasterDictionary<int, IEntityPrimaryKey> IEntityTable.PrimaryKeys => primaryKeys;

        internal Table() { }

        protected abstract EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
        protected abstract void Swap(IEntityFunctions functions, in EGID egid, in ExclusiveBuildGroup groupID);
        protected abstract void Remove(IEntityFunctions functions, in EGID egid);

        EntityInitializer IEntityTable.Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors)
            => Build(factory, entityID, implementors);

        void IEntityTable.Swap(IEntityFunctions functions, in EGID egid, in ExclusiveBuildGroup groupID)
            => Swap(functions, egid, groupID);

        /// <summary>
        /// Schema extensions only support remove from untyped table for now
        /// Because it is not possible to determine if it is safe to swap to other table
        /// Primary Key should be used instead of swap if needed.
        /// </summary>
        void IEntityTable.Remove(IEntityFunctions functions, in EGID egid)
            => Remove(functions, egid);
    }
}

namespace Svelto.ECS.Schema
{
    public sealed class Table<TRow> : Table, IEntityTable<TRow>, IEntityTables<TRow>
        where TRow : DescriptorRow<TRow>
    {
        private delegate void InitializerDelegate(in EntityInitializer initializer);

        private FasterList<InitializerDelegate> _defaultInitializerActions;

        public void AddPrimaryKeys(params IPrimaryKeyProvider<TRow>[] primaryKeys)
        {
            foreach (var primaryKey in primaryKeys)
                this.primaryKeys.Add(primaryKey.PrimaryKeyID, primaryKey);
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

        protected override void Swap(IEntityFunctions functions, in EGID egid, in ExclusiveBuildGroup groupID)
        {
            functions.SwapEntityGroup<DescriptorRow<TRow>.Descriptor>(egid, groupID);
        }

        protected override void Remove(IEntityFunctions functions, in EGID egid)
        {
            functions.RemoveEntity<DescriptorRow<TRow>.Descriptor>(egid);
        }

        int IEntityTables.Range => 1;

        IEntityTable IEntityTables.GetTable(int index) => this;

        IEntityTable<TRow> IEntityTables<TRow>.GetTable(int index) => this;

        IEnumerable<IEntityTable<TRow>> IEntityTablesBuilder<TRow>.Tables
        {
            get { yield return this; }
        }
    }
}