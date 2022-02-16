using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public static partial class GroupExtensions
    {
        public static FasterList<T> ToFasterList<T>(this IEnumerable<T> enumerable)
        {
            return new FasterList<T>(enumerable.ToArray());
        }

        // this basically prevent putting wrong descriptor to a group
        public static EntityInitializer BuildEntity<T>(this IEntityFactory factory,
                uint entityID, Group<T> group, IEnumerable<object> implementors = null)
            where T : IEntityDescriptor, new()
        {
            return factory.BuildEntity<T>(entityID, group.exclusiveGroup, implementors);
        }

        public static void RemoveEntity<T>(this IEntityFunctions functions, uint entityID, in Group<T> group)
            where T : IEntityDescriptor, new()
        {
            functions.RemoveEntity<T>(entityID, group.exclusiveGroup);
        }

        public static void RemoveEntitiesFromGroup<T>(this IEntityFunctions functions, in Group<T> group)
            where T : IEntityDescriptor, new()
        {
            functions.RemoveEntitiesFromGroup(group.exclusiveGroup);
        }

        public static void SwapEntitiesInGroup<T>(this IEntityFunctions functions, in Group<T> fromGroup, in Group<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntitiesInGroup<T>(fromGroup.exclusiveGroup, toGroup.exclusiveGroup);
        }

        public static void SwapEntityGroup<T>(this IEntityFunctions functions, uint entityID, in Group<T> fromGroup, in Group<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(entityID, fromGroup.exclusiveGroup, toGroup.exclusiveGroup);
        }

        public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, in Group<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(fromID, toGroup.exclusiveGroup);
        }

        public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, in Group<T> fromGroup, in Group<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(fromID, fromGroup.exclusiveGroup, toGroup.exclusiveGroup);
        }

        public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, EGID toID, in Group<T> mustBeFromGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(fromID, toID, mustBeFromGroup.exclusiveGroup);
        }
    }
}