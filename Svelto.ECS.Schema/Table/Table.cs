using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public sealed partial class Table<T> : ISchemaDefinitionTable
        where T : IEntityDescriptor, new()
    {
        private readonly ExclusiveGroupStruct _exclusiveGroup;

        public ref readonly ExclusiveGroupStruct ExclusiveGroup => ref _exclusiveGroup;

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
            return new TablesBuilder<T>(new Table<T>[] { a, b });
        }

        public static implicit operator ExclusiveGroupStruct(in Table<T> group) => group._exclusiveGroup;

    }
}