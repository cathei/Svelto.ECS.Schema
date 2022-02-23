using System.Runtime.CompilerServices;

namespace Svelto.ECS.Schema
{
    public readonly partial struct Group<T> where T : IEntityDescriptor, new()
    {
        public readonly ExclusiveGroupStruct exclusiveGroup;

        public Group(in ExclusiveGroupStruct group)
        {
            exclusiveGroup = group;
        }

        public static GroupsBuilder<T> operator+(in Group<T> a, in Group<T> b)
        {
            return new GroupsBuilder<T>(new [] { a.exclusiveGroup, b.exclusiveGroup });
        }

        public static implicit operator ExclusiveGroupStruct(in Group<T> group) => group.exclusiveGroup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityInitializer Build(IEntityFactory factory, uint entityID)
        {
            return factory.BuildEntity<T>(entityID, exclusiveGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IEntityFunctions functions, EGID fromID)
        {
            functions.SwapEntityGroup<T>(fromID, exclusiveGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IEntityFunctions functions, uint entityID)
        {
            functions.RemoveEntity<T>(entityID, exclusiveGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TR Entity<TR>(EntitiesDB entitiesDB, uint entityID)
            where TR : unmanaged, IEntityComponent
        {
            return ref entitiesDB.QueryEntity<TR>(entityID, exclusiveGroup);
        }
    }
}