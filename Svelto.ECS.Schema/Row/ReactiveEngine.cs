using Svelto.ECS;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IReactRowAdd<TRow, TComponent> : IReactOnAddEx<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    {
        IndexedDB indexedDB { get; }

        void Add(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group);

        void IReactOnAddEx<TComponent>.Add(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable<TRow>(groupID);
            if (table == null)
                return;

            Add(collection, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), groupID);
        }
    }

    public interface IReactRowRemove<TRow, TComponent> : IReactOnRemoveEx<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    {
        IndexedDB indexedDB { get; }

        void Remove(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group);

        void IReactOnRemoveEx<TComponent>.Remove(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable<TRow>(groupID);
            if (table == null)
                return;

            Remove(collection, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), groupID);
        }
    }

    public interface IReactRowSwap<TRow, TComponent> : IReactOnSwapEx<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    {
        IndexedDB indexedDB { get; }

        void MovedTo(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup);

        void IReactOnSwapEx<TComponent>.MovedTo(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TComponent> collection,
            ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            var fromTable = indexedDB.FindTable<TRow>(fromGroup);
            var toTable = indexedDB.FindTable<TRow>(toGroup);

            // return if neither of group is Table<TRow>
            if (fromTable == null && toTable == null)
                return;

            MovedTo(collection, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), fromGroup, toGroup);
        }
    }
}
