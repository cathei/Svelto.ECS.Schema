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

        public static EntityInitializer Build<TRow>(this IEntityFactory factory, IEntityTable<TRow> table,
                uint entityID, IEnumerable<object> implementors = null)
            where TRow : DescriptorRow<TRow>
        {
            return table.Build(factory, entityID, implementors);
        }

        /// <summary>
        /// Throws exception if Row type mismatches
        /// </summary>
        public static void Move<TRow>(
                this IndexedDB indexedDB, IEntityTable<TRow> table, uint entityID, in ExclusiveGroupStruct groupID)
            where TRow : DescriptorRow<TRow>
        {
            if (indexedDB.FindTable<TRow>(groupID) == null)
                throw new ECSException("Row type mismatch");

            indexedDB.entityFunctions.SwapEntityGroup<DescriptorRow<TRow>.Descriptor>(entityID, groupID, table.Group);
        }

        /// <summary>
        /// Remove entity from table
        /// </summary>
        public static void Remove(
            this IndexedDB indexedDB, uint entityID, in ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable(groupID);
            // this is supported with virtual fuction so you can delete with any row
            table.Remove(indexedDB.entityFunctions, entityID, groupID);
        }

        /// <summary>
        /// Remove all entities from table
        /// </summary>
        public static void RemoveAll<TR>(
                this IndexedDB indexedDB, IEntityTable<TR> table)
            where TR : class, IEntityRow
        {
            for (uint i = 0; i < table.GroupRange; ++i)
            {
                indexedDB.entityFunctions.RemoveEntitiesFromGroup(table.Group + i);
            }
        }
    }
}
