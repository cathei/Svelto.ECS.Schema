using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct MemoQuery<TRow, TComponent>
        where TRow : class, IIndexableRow<TComponent>
        where TComponent : unmanaged, IEntityComponent
    {
        public readonly IndexedDB _indexedDB;
        public readonly MemoBase<TRow, TComponent> _memo;

        public MemoQuery(IndexedDB indexedDB, MemoBase<TRow, TComponent> memo)
        {
            _indexedDB = indexedDB;
            _memo = memo;
        }

        public MemoQuery<TRow, TComponent> Add(uint entityID, in ExclusiveGroupStruct groupID)
        {
            _indexedDB.AddMemo(_memo, entityID, groupID);
            return this;
        }

        public MemoQuery<TRow, TComponent> Remove(uint entityID, in ExclusiveGroupStruct groupID)
        {
            _indexedDB.RemoveMemo(_memo, entityID, groupID);
            return this;
        }

        public MemoQuery<TRow, TComponent> Clear()
        {
            _indexedDB.ClearMemo(_memo);
            return this;
        }

        public MemoQuery<TRow, TComponent> Set<TIndex>(TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            _memo.Set(_indexedDB, indexQuery);
            return this;
        }

        public MemoQuery<TRow, TComponent> Union<TIndex>(TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            _memo.Union(_indexedDB, indexQuery);
            return this;
        }

        public MemoQuery<TRow, TComponent> Intersect<TIndex>(TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            _memo.Intersect(_indexedDB, indexQuery);
            return this;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public static class MemoExtensions
    {
        public static MemoQuery<TRow, TComponent> Memo<TRow, TComponent>(
                this IndexedDB indexedDB, MemoBase<TRow, TComponent> memo)
            where TRow : class, IIndexableRow<TComponent>
            where TComponent : unmanaged, IEntityComponent
        {
            return new MemoQuery<TRow, TComponent>(indexedDB, memo);
        }
    }
}
