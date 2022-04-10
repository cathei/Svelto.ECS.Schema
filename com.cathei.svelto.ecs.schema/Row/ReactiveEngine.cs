using Svelto.ECS;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    ///<summary>
    /// Only reason this class exist is Unity has a bug with interface default implementation
    /// https://forum.unity.com/threads/unity-c-8-support.663757/page-9#post-8036273
    /// We need to reduce number of generic arguments
    ///</summary>
    public interface IReactRowEngine
    {
        IndexedDB indexedDB { get; }

        ITableDefinition FindTable(in ExclusiveGroupStruct groupID);
    }

    public interface IReactRowEngine<TRow> : IReactRowEngine
        where TRow : class, IEntityRow
    {
        ITableDefinition IReactRowEngine.FindTable(in ExclusiveGroupStruct groupID)
            => indexedDB.FindTable<TRow>(groupID);
    }

    public interface IReactRowAdd<TComponent> : IReactRowEngine, IReactOnAddEx<TComponent>
        where TComponent : struct, IEntityComponent
    {
        void Add(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group);

        void IReactOnAddEx<TComponent>.Add(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var table = FindTable(groupID);
            if (table == null)
                return;

            Add(collection, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), groupID);
        }
    }

    public interface IReactRowRemove<TComponent> : IReactRowEngine, IReactOnRemoveEx<TComponent>
        where TComponent : struct, IEntityComponent
    {
        void Remove(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct group);

        void IReactOnRemoveEx<TComponent>.Remove(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var table = FindTable(groupID);
            if (table == null)
                return;

            Remove(collection, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), groupID);
        }
    }

    public interface IReactRowSwap<TComponent> : IReactRowEngine, IReactOnSwapEx<TComponent>
        where TComponent : struct, IEntityComponent
    {
        void MovedTo(in EntityCollection<TComponent> collection, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup);

        void IReactOnSwapEx<TComponent>.MovedTo(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TComponent> collection,
            ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            var fromTable = FindTable(fromGroup);
            var toTable = FindTable(toGroup);

            // return if neither of group is Table<TRow>
            if (fromTable == null && toTable == null)
                return;

            MovedTo(collection, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), fromGroup, toGroup);
        }
    }
}

namespace Svelto.ECS.Schema
{
    public interface IReactRowAdd<TRow, TComponent> : IReactRowEngine<TRow>, IReactRowAdd<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    { }

    public interface IReactRowRemove<TRow, TComponent> : IReactRowEngine<TRow>, IReactRowRemove<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    { }

    public interface IReactRowSwap<TRow, TComponent> : IReactRowEngine<TRow>, IReactRowSwap<TComponent>
        where TRow : class, IReactiveRow<TComponent>
        where TComponent : struct, IEntityComponent
    { }
}
