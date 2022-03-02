using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static partial class TableExtensions
    {
        public static FasterList<T> ToFasterList<T>(this IEnumerable<T> enumerable)
        {
            return new FasterList<T>(enumerable.ToArray());
        }
    }

        // this basically prevent putting wrong descriptor to a group
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static EntityInitializer BuildEntity<T>(this IEntityFactory factory,
    //             uint entityID, Table<T> group, IEnumerable<object> implementors = null)
    //         where T : IEntityRow
    //     {
    //         return factory.BuildEntity<RowDescriptor<T>>(entityID, group.ExclusiveGroup, implementors);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void RemoveEntity<T>(this IEntityFunctions functions, uint entityID, in Table<T> group)
    //         where T : IEntityRow
    //     {
    //         functions.RemoveEntity<RowDescriptor<T>>(entityID, group.ExclusiveGroup);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void RemoveEntitiesFromGroup<T>(this IEntityFunctions functions, in Table<T> group)
    //         where T : IEntityRow
    //     {
    //         functions.RemoveEntitiesFromGroup(group.ExclusiveGroup);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void SwapEntitiesInGroup<T>(this IEntityFunctions functions, in Table<T> fromGroup, in Table<T> toGroup)
    //         where T : IEntityRow
    //     {
    //         functions.SwapEntitiesInGroup<RowDescriptor<T>>(fromGroup.ExclusiveGroup, toGroup.ExclusiveGroup);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void SwapEntityGroup<T>(this IEntityFunctions functions, uint entityID, in Table<T> fromGroup, in Table<T> toGroup)
    //         where T : IEntityRow
    //     {
    //         functions.SwapEntityGroup<RowDescriptor<T>>(entityID, fromGroup.ExclusiveGroup, toGroup.ExclusiveGroup);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, in Table<T> toGroup)
    //         where T : IEntityRow
    //     {
    //         functions.SwapEntityGroup<RowDescriptor<T>>(fromID, toGroup.ExclusiveGroup);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, in Table<T> fromGroup, in Table<T> toGroup)
    //         where T : IEntityRow
    //     {
    //         functions.SwapEntityGroup<RowDescriptor<T>>(fromID, fromGroup.ExclusiveGroup, toGroup.ExclusiveGroup);
    //     }

    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static void SwapEntityGroup<T>(this IEntityFunctions functions, EGID fromID, EGID toID, in Table<T> mustBeFromGroup)
    //         where T : IEntityRow
    //     {
    //         functions.SwapEntityGroup<RowDescriptor<T>>(fromID, toID, mustBeFromGroup.ExclusiveGroup);
    //     }
    // }

    // public static class TableNativeExtensions
    // {
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static ref T Entity<T>(this IEntityTable table, EntitiesDB entitiesDB, uint entityID)
    //         where T : unmanaged, IEntityComponent
    //     {
    //         return ref entitiesDB.QueryEntity<T>(entityID, table.ExclusiveGroup);
    //     }
    // }

    // public static class TableManagedExtensions
    // {
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public static ref T Entity<T>(this IEntityTable table, EntitiesDB entitiesDB, uint entityID)
    //         where T : struct, IEntityViewComponent
    //     {
    //         return ref entitiesDB.QueryEntity<T>(entityID, table.ExclusiveGroup);
    //     }
    // }
}