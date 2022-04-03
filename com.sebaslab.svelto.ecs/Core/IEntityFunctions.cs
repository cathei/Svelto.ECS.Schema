namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specifying the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(uint entityID, ExclusiveBuildGroup groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid) where T : IEntityDescriptor, new();
        
        void RemoveEntitiesFromGroup(ExclusiveBuildGroup groupID);

        void SwapEntitiesInGroup<T>(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID)  where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(uint entityID, ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID)
            where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, ExclusiveBuildGroup toGroupID) where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, ExclusiveBuildGroup fromGroup, ExclusiveBuildGroup toGroupID)
            where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, EGID toId) where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, EGID toId, ExclusiveBuildGroup mustBeFromGroup)
            where T : IEntityDescriptor, new();
#if UNITY_NATIVE
        Svelto.ECS.Native.NativeEntityRemove                 ToNativeRemove<T>(string memberName)  where T : IEntityDescriptor, new();
        Svelto.ECS.Native.NativeEntitySwap ToNativeSwap<T>(string memberName)  where T : IEntityDescriptor, new();
#endif        
    }
}