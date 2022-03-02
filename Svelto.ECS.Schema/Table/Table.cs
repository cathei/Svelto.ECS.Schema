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

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public bool Exists<T>(EntitiesDB entitiesDB, uint entityID) where T : struct, IEntityComponent
        //     => entitiesDB.Exists<T>(entityID, _exclusiveGroup);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public int Count<T>(EntitiesDB entitiesDB) where T : struct, IEntityComponent
        //     => entitiesDB.Count<T>(_exclusiveGroup);

        public static implicit operator ExclusiveGroupStruct(in TableBase group) => group._exclusiveGroup;
    }

    public interface IEntityTable<out TRow> : IEntityTable where TRow : IEntityRow
    {
        EntityInitializer Build(IEntityFactory factory, uint entityID);
        void Insert(IEntityFunctions functions, EGID fromID);
    }

    public abstract class TableBase<TRow> : TableBase, IEntityTable<TRow>, IEntityTablesBuilder<TRow>
        where TRow : DescriptorRow<TRow>
    {
        internal TableBase() : base() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityInitializer Build(IEntityFactory factory, uint entityID)
            => factory.BuildEntity<DescriptorRow<TRow>.Descriptor>(entityID, _exclusiveGroup);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IEntityFunctions functions, EGID fromID)
            => functions.SwapEntityGroup<DescriptorRow<TRow>.Descriptor>(fromID, _exclusiveGroup);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IEntityFunctions functions, uint entityID)
            => functions.RemoveEntity<DescriptorRow<TRow>.Descriptor>(entityID, _exclusiveGroup);

        IEnumerable<IEntityTable<TRow>> IEntityTablesBuilder<TRow>.Tables
        {
            get { yield return this; }
        }
    }
}