using Svelto.ECS;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IReactRowAdd<TRow, TResultSet> : IReactOnAddEx<RowIdentityComponent>
        where TRow : class, IQueryableRow<TResultSet>
        where TResultSet : struct, IResultSet
    {
        IndexedDB indexedDB { get; }

        void Add(in TResultSet resultSet, RangedIndices indices, ExclusiveGroupStruct group);

        void IReactOnAddEx<RowIdentityComponent>.Add(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<RowIdentityComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable<TRow>(groupID);
            if (table == null)
                return;

            ResultSetHelper<TResultSet>.Assign(out var result, indexedDB.entitiesDB, groupID);

            Add(result, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), groupID);
        }
    }

    public interface IReactRowRemove<TRow, TResultSet> : IReactOnRemoveEx<RowIdentityComponent>
        where TRow : class, IQueryableRow<TResultSet>
        where TResultSet : struct, IResultSet
    {
        IndexedDB indexedDB { get; }

        void Remove(in TResultSet resultSet, RangedIndices indices, ExclusiveGroupStruct group);

        void IReactOnRemoveEx<RowIdentityComponent>.Remove(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<RowIdentityComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var table = indexedDB.FindTable<TRow>(groupID);
            if (table == null)
                return;

            ResultSetHelper<TResultSet>.Assign(out var result, indexedDB.entitiesDB, groupID);

            Remove(result, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), groupID);
        }
    }

    public interface IReactRowSwap<TRow, TResultSet> : IReactOnSwapEx<RowIdentityComponent>
        where TRow : class, IQueryableRow<TResultSet>
        where TResultSet : struct, IResultSet
    {
        IndexedDB indexedDB { get; }

        void MovedTo(in TResultSet resultSet, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup);

        void IReactOnSwapEx<RowIdentityComponent>.MovedTo(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<RowIdentityComponent> collection,
            ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            var fromTable = indexedDB.FindTable<TRow>(fromGroup);
            var toTable = indexedDB.FindTable<TRow>(toGroup);

            if (fromTable == null || toTable == null)
                return;

            ResultSetHelper<TResultSet>.Assign(out var result, indexedDB.entitiesDB, toGroup);

            MovedTo(result, new(rangeOfEntities.start, rangeOfEntities.end - rangeOfEntities.start), fromGroup, toGroup);
        }
    }
}
