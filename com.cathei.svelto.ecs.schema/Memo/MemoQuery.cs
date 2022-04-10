using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct MemoQuery<TRow>
        where TRow : class, IEntityRow
    {
        public readonly IndexedDB _indexedDB;
        public readonly MemoBase<TRow> _memo;

        public MemoQuery(IndexedDB indexedDB, MemoBase<TRow> memo)
        {
            _indexedDB = indexedDB;
            _memo = memo;
        }

        public MemoQuery<TRow> Add(uint entityID, in ExclusiveGroupStruct groupID)
        {
            _indexedDB.AddMemo(_memo, new EGID(entityID, groupID));
            return this;
        }

        public MemoQuery<TRow> Remove(uint entityID, in ExclusiveGroupStruct groupID)
        {
            _indexedDB.RemoveMemo(_memo, new EGID(entityID, groupID));
            return this;
        }

        public MemoQuery<TRow> Add(in EGID egid)
        {
            _indexedDB.AddMemo(_memo, egid);
            return this;
        }

        public MemoQuery<TRow> Remove(in EGID egid)
        {
            _indexedDB.RemoveMemo(_memo, egid);
            return this;
        }

        public MemoQuery<TRow> Clear()
        {
            _indexedDB.ClearMemo(_memo);
            return this;
        }

        public MemoQuery<TRow> Set<TIndex>(TIndex indexQuery)
            where TIndex : IWhereQuery<TRow>
        {
            _memo.Set(_indexedDB, indexQuery);
            return this;
        }

        public MemoQuery<TRow> Union<TIndex>(TIndex indexQuery)
            where TIndex : IWhereQuery<TRow>
        {
            _memo.Union(_indexedDB, indexQuery);
            return this;
        }

        public MemoQuery<TRow> Intersect<TIndex>(TIndex indexQuery)
            where TIndex : IWhereQuery<TRow>
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
        public static MemoQuery<TRow> Memo<TRow>(
                this IndexedDB indexedDB, MemoBase<TRow> memo)
            where TRow : class, IEntityRow
        {
            return new MemoQuery<TRow>(indexedDB, memo);
        }
    }
}
