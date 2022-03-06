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
            return factory.BuildEntity<DescriptorRow<TRow>.Descriptor>(
                entityID, table.ExclusiveGroup, implementors);
        }

        /// <summary>
        /// Query entrypoint Move -> To
        /// </summary>
        public static (IEntityFunctions, IEntityTable<TRow>, uint) Move<TRow>(
                this IEntityFunctions functions, IEntityTable<TRow> table, uint entityID)
            where TRow : DescriptorRow<TRow>
        {
            return (functions, table, entityID);
        }

        /// <summary>
        /// Move(fromGroup, fromID).To(toGroup, toID)
        /// </summary>
        public static void To<TRow>(this (IEntityFunctions, IEntityTable<TRow>, uint) query,
                IEntityTable<TRow> table, uint entityID)
            where TRow : DescriptorRow<TRow>
        {
            query.Item1.SwapEntityGroup<DescriptorRow<TRow>.Descriptor>(
                new EGID(query.Item3, query.Item2.ExclusiveGroup),
                new EGID(entityID, table.ExclusiveGroup));
        }

        /// <summary>
        /// Move(fromGroup, fromID).To(toGroup)
        /// no entity ID means it will use previous entity ID
        /// </summary>
        public static void To<TRow>(this (IEntityFunctions, IEntityTable<TRow>, uint) query,
                IEntityTable<TRow> table)
            where TRow : DescriptorRow<TRow>
        {
            query.Item1.SwapEntityGroup<DescriptorRow<TRow>.Descriptor>(
                query.Item3, query.Item2.ExclusiveGroup, table.ExclusiveGroup);
        }

        /// <summary>
        /// Query entrypoint MoveAll -> To
        /// No entity id means it will move whole group
        /// </summary>
        public static (IEntityFunctions, IEntityTable<TRow>) MoveAll<TRow>(
                this IEntityFunctions functions, IEntityTable<TRow> table)
            where TRow : DescriptorRow<TRow>
        {
            return (functions, table);
        }

        /// <summary>
        /// MoveAll(fromGroup).To(toGroup)
        /// no entity ID means it will use previous entity ID
        /// </summary>
        public static void To<TRow>(this (IEntityFunctions, IEntityTable<TRow>) query,
                IEntityTable<TRow> table)
            where TRow : DescriptorRow<TRow>
        {
            query.Item1.SwapEntitiesInGroup<DescriptorRow<TRow>.Descriptor>(
                query.Item2.ExclusiveGroup, table.ExclusiveGroup);
        }

        /// <summary>
        /// Remove entity from table
        /// </summary>
        public static void Remove<TRow>(
                this IEntityFunctions functions, IEntityTable<TRow> table, uint entityID)
            where TRow : DescriptorRow<TRow>
        {
            functions.RemoveEntity<DescriptorRow<TRow>.Descriptor>(entityID, table.ExclusiveGroup);
        }

        /// <summary>
        /// Remove all entity from table
        /// </summary>
        public static void RemoveAll<TRow>(
                this IEntityFunctions functions, IEntityTable<TRow> table)
            where TRow : DescriptorRow<TRow>
        {
            functions.RemoveEntitiesFromGroup(table.ExclusiveGroup);
        }
    }
}