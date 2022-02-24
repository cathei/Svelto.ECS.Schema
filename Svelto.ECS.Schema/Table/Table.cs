using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Svelto.ECS.Schema.Definition
{
    public sealed class Table<T> : IEntitySchemaTable where T : IEntityDescriptor, new()
    {
        private readonly ExclusiveGroup _exclusiveGroup;
        public ref readonly ExclusiveGroup ExclusiveGroup => ref _exclusiveGroup;

        public Table()
        {
            _exclusiveGroup = new ExclusiveGroup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityInitializer Build(IEntityFactory factory, uint entityID)
        {
            return factory.BuildEntity<T>(entityID, _exclusiveGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IEntityFunctions functions, EGID fromID)
        {
            functions.SwapEntityGroup<T>(fromID, _exclusiveGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IEntityFunctions functions, uint entityID)
        {
            functions.RemoveEntity<T>(entityID, _exclusiveGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TR Entity<TR>(EntitiesDB entitiesDB, uint entityID)
            where TR : unmanaged, IEntityComponent
        {
            return ref entitiesDB.QueryEntity<TR>(entityID, _exclusiveGroup);
        }

        public static TablesBuilder<T> operator+(in Table<T> a, in Table<T> b)
        {
            return new TablesBuilder<T>(new ExclusiveGroupStruct[] { a._exclusiveGroup, b._exclusiveGroup });
        }

        public static implicit operator ExclusiveGroupStruct(in Table<T> group) => group._exclusiveGroup;

    }
}