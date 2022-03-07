using Svelto.ECS;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IReactRowAddAndRemove<in TRow, TComponent> : IReactOnAddAndRemove<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    {
        IndexedDB indexedDB { get; }

        void Add(ref TComponent entityComponent, IEntityTable<TRow> table, uint entityID);
        void Remove(ref TComponent entityComponent, IEntityTable<TRow> table, uint entityID);

        void IReactOnAddAndRemove<TComponent>.Add(ref TComponent component, EGID egid)
        {
            var table = indexedDB.FindTable<TRow>(egid.groupID);
            if (table == null)
                return;

            Add(ref component, table, egid.entityID);
        }

        void IReactOnAddAndRemove<TComponent>.Remove(ref TComponent component, EGID egid)
        {
            var table = indexedDB.FindTable<TRow>(egid.groupID);
            if (table == null)
                return;

            Remove(ref component, table, egid.entityID);
        }
    }

    public interface IReactRowSwap<in TRow, TComponent> : IReactOnSwap<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    {
        IndexedDB indexedDB { get; }

        void IReactOnSwap<TComponent>.MovedTo(ref TComponent component, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            var previousTable = indexedDB.FindTable<TRow>(previousGroup);
            var table = indexedDB.FindTable<TRow>(egid.groupID);

            if (previousTable == null || table == null)
                return;

            MovedTo(ref component, previousTable, table, egid.entityID);
        }

        void MovedTo(ref TComponent entityComponent, IEntityTable<TRow> previousTable, IEntityTable<TRow> table, uint entityID);
    }
}
