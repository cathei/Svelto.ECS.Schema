using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    public static partial class TableExtensions
    {
        public static FasterList<T> ToFasterList<T>(this IEnumerable<T> enumerable)
        {
            return new FasterList<T>(enumerable.ToArray());
        }

        // this basically prevent putting wrong descriptor to a group
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityInitializer BuildEntity<T>(this IEntityFactory factory,
                uint entityID, Table<T> group, IEnumerable<object> implementors = null)
            where T : IEntityDescriptor, new()
        {
            return factory.BuildEntity<T>(entityID, group.ExclusiveGroupStruct, implementors);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntity<T>(this IEntityFunctions functions, uint entityID, in Table<T> group)
            where T : IEntityDescriptor, new()
        {
            functions.RemoveEntity<T>(entityID, group.ExclusiveGroupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntitiesFromGroup<T>(this IEntityFunctions functions, in Table<T> group)
            where T : IEntityDescriptor, new()
        {
            functions.RemoveEntitiesFromGroup(group.ExclusiveGroupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntitiesInGroup<T>(this IEntityFunctions functions, in Table<T> fromGroup, in Table<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntitiesInGroup<T>(fromGroup.ExclusiveGroupStruct, toGroup.ExclusiveGroupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntityGroup<T>(this IEntityFunctions functions, uint entityID, in Table<T> fromGroup, in Table<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(entityID, fromGroup.ExclusiveGroupStruct, toGroup.ExclusiveGroupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, in Table<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(fromID, toGroup.ExclusiveGroupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, in Table<T> fromGroup, in Table<T> toGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(fromID, fromGroup.ExclusiveGroupStruct, toGroup.ExclusiveGroupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, EGID toID, in Table<T> mustBeFromGroup)
            where T : IEntityDescriptor, new()
        {
            functions.SwapEntityGroup<T>(fromID, toID, mustBeFromGroup.ExclusiveGroupStruct);
        }
    }
}gw