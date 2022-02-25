using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    internal static class GlobalMemoCount
    {
        private static int Count = 0;

        public static int Generate() => Interlocked.Increment(ref Count);
    }

    public sealed class Memo<T> : IEntitySchemaMemo, IIndexQuery
        where T : unmanaged, IEntityComponent
    {
        // equvalent to ExclusiveGroupStruct.Generate()
        internal readonly int _memoID = GlobalMemoCount.Generate();

        int IEntitySchemaMemo.MemoID => _memoID;

        public void Add(IndexesDB indexesDB, EGID egid)
        {
            indexesDB.AddMemo(this, egid.entityID, egid.groupID);
        }

        public void Remove(IndexesDB indexesDB, EGID egid)
        {
            indexesDB.RemoveMemo(this, egid.entityID, egid.groupID);
        }

        public void Clear(IndexesDB indexesDB)
        {
            indexesDB.ClearMemo(this);
        }

        IndexesDB.IndexerSetData IIndexQuery.GetGroupIndexDataList(IndexesDB indexesDB)
        {
            indexesDB.memos.TryGetValue(_memoID, out var result);
            return result;
        }
    }
}