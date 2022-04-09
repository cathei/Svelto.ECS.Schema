using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class ForeignKeyExtensions
    {
        public static void Update<TComponent>(
                this IndexedDB indexedDB, ref TComponent component, in EGID egid)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            indexedDB.UpdateForeignKeyComponent<TComponent>(egid, component.reference);
        }

        public static void Update<TComponent>(
                this IndexedDB indexedDB, ref TComponent component, in EGID egid, in EntityReference reference)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            component.reference = reference;
            indexedDB.UpdateForeignKeyComponent<TComponent>(egid, component.reference);
        }

        public static void Update<TComponent>(
                this IndexedDB indexedDB, ref TComponent component, in EGID egid, in EGID other)
            where TComponent : unmanaged, IForeignKeyComponent
        {
            component.reference = indexedDB.GetEntityReference(other);
            indexedDB.UpdateForeignKeyComponent<TComponent>(egid, component.reference);
        }
    }
}
