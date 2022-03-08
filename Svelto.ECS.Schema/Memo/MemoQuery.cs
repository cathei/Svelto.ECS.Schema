using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct MemoQuery<TRow, TComponent>
        where TRow : class, IIndexableRow<TComponent>
        where TComponent : unmanaged, IEntityComponent, INeedEGID
    {
        public readonly IndexedDB _indexedDB;
        public readonly MemoBase<TRow, TComponent> _memo;

        public MemoQuery(IndexedDB indexedDB, MemoBase<TRow, TComponent> memo)
        {
            _indexedDB = indexedDB;
            _memo = memo;
        }

        public void Add(TComponent component)
        {
            var table = _indexedDB.FindTable<TRow>(component.ID.groupID);
            _indexedDB.AddMemo(_memo, component.ID.entityID, table);
        }

        public void Remove(TComponent component)
        {
            var table = _indexedDB.FindTable<TRow>(component.ID.groupID);
            _indexedDB.RemoveMemo(_memo, component.ID.entityID, table);
        }

        public void Clear()
        {
            _indexedDB.ClearMemo(_memo);
        }

        public void Set<TIndex>(TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            _memo.Set(_indexedDB, indexQuery);
        }

        public void Union<TIndex>(TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            _memo.Union(_indexedDB, indexQuery);
        }

        public void Intersect<TIndex>(TIndex indexQuery)
            where TIndex : IIndexQuery<TRow>
        {
            _memo.Intersect(_indexedDB, indexQuery);
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
            where TComponent : unmanaged, IEntityComponent, INeedEGID
        {
            return new MemoQuery<TRow, TComponent>(indexedDB, memo);
        }
    }
}
