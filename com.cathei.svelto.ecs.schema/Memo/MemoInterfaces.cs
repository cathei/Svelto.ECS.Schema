using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IEntityMemo<out TRow> : IWhereQuery<IEntityRow>
        where TRow : class, IEntityRow
    {
        internal ref EntityFilterCollection GetFilter(IndexedDB indexedDB);

        internal void Add(IndexedDB indexedDB, in EGID egid);
        internal void Remove(IndexedDB indexedDB, in EGID egid);
        internal void Clear(IndexedDB indexedDB);

        internal void Set<TWhere>(IndexedDB indexedDB, TWhere indexQuery) where TWhere : IWhereQuery<TRow>;
        internal void Union<TWhere>(IndexedDB indexedDB, TWhere indexQuery) where TWhere : IWhereQuery<TRow>;
        internal void Intersect<TWhere>(IndexedDB indexedDB, TWhere other) where TWhere : IWhereQuery<TRow>;
    }
}