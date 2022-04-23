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
                this IndexedDB indexedDB, in EGID egid, IEntityTable<TRow> toTable)
            where TRow : DescriptorRow<TRow>
        {
            if (indexedDB.FindTable<TRow>(egid.groupID) == null)
                throw new ECSException("Row type mismatch");

            indexedDB.entityFunctions.SwapEntityGroup<DescriptorRow<TRow>.Descriptor>(egid, toTable.Group);
        }

        /// <summary>
        /// Remove entity from table
        /// </summary>
        public static void Remove(
            this IndexedDB indexedDB, in EGID egid)
        {
            var table = indexedDB.FindTable(egid.groupID);
            // this is supported with virtual fuction so you can delete with any row
            table.Remove(indexedDB.entityFunctions, egid);
        }

        /// <summary>
        /// Remove all entities from table
        /// </summary>
        public static void RemoveAll<TRow>(
                this IndexedDB indexedDB, IEntityTable<TRow> table)
            where TRow : class, IEntityRow
        {
            for (uint i = 0; i < table.GroupRange; ++i)
            {
                indexedDB.entityFunctions.RemoveEntitiesFromGroup(table.Group + i);
            }
        }
    }
}
