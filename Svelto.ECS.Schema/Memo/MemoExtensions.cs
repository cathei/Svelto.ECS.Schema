using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public static class MemoExtensions
    {
        public static (IndexedDB, TMemo) Memo<TMemo>(this IndexedDB indexedDB, TMemo memo)
            where TMemo : MemoBase
        {
            return (indexedDB, memo);
        }

        public static void Add<TR, TC>(this (IndexedDB, MemoBase<TR, TC>) query, TC component)
            where TR : IEntityRow<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            var table = query.Item1.FindTable<TR>(component.ID.groupID);
            query.Item1.AddMemo(query.Item2, component.ID.entityID, table);
        }

        public static void Remove<TR, TC>(this (IndexedDB, MemoBase<TR, TC>) query, TC component)
            where TR : IEntityRow<TC>
            where TC : unmanaged, IEntityComponent, INeedEGID
        {
            var table = query.Item1.FindTable<TR>(component.ID.groupID);
            query.Item1.RemoveMemo(query.Item2, component.ID.entityID, table);
        }

        public static void Clear(this (IndexedDB, MemoBase) query)
        {
            query.Item1.ClearMemo(query.Item2);
        }

        public static void Set<TR, TC, TMR, TMC>(this (IndexedDB, MemoBase<TR, TC>) query, MemoBase<TMR, TMC> memo)
            where TR : IEntityRow<TC>, TMR
            where TC : unmanaged, IEntityComponent, INeedEGID
            where TMR : IEntityRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            query.Item2.Set<IIndexQuery<TMR>, TMR>(query.Item1, memo);
        }

        public static void Union<TR, TC, TMR, TMC>(this (IndexedDB, MemoBase<TR, TC>) query, MemoBase<TMR, TMC> memo)
            where TR : IEntityRow<TC>, TMR
            where TC : unmanaged, IEntityComponent, INeedEGID
            where TMR : IEntityRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            query.Item2.Union<IIndexQuery<TMR>, TMR>(query.Item1, memo);
        }

        public static void Intersect<TR, TC, TMR, TMC>(this (IndexedDB, MemoBase<TR, TC>) query, MemoBase<TMR, TMC> memo)
            where TR : IEntityRow<TC>, TMR
            where TC : unmanaged, IEntityComponent, INeedEGID
            where TMR : IEntityRow<TMC>
            where TMC : unmanaged, IEntityComponent, INeedEGID
        {
            query.Item2.Intersect<IIndexQuery<TMR>, TMR>(query.Item1, memo);
        }

        public static void Set<TR, TC, TIR, TIK, TIC>(this (IndexedDB, MemoBase<TR, TC>) query,
                IIndexQueryable<TIR, TIK, TIC> index, in TIK key)
            where TR : IEntityRow<TC>, TIR
            where TC : unmanaged, IEntityComponent, INeedEGID
            where TIR : class, IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            query.Item2.Set<IndexQuery<TIR, TIK, TIC>, TIR>(query.Item1, index.Query(key));
        }

        public static void Union<TR, TC, TIR, TIK, TIC>(this (IndexedDB, MemoBase<TR, TC>) query,
                IIndexQueryable<TIR, TIK, TIC> index, in TIK key)
            where TR : IEntityRow<TC>, TIR
            where TC : unmanaged, IEntityComponent, INeedEGID
            where TIR : class, IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            query.Item2.Union<IndexQuery<TIR, TIK, TIC>, TIR>(query.Item1, index.Query(key));
        }

        public static void Intersect<TR, TC, TIR, TIK, TIC>(this (IndexedDB, MemoBase<TR, TC>) query,
                IIndexQueryable<TIR, TIK, TIC> index, in TIK key)
            where TR : IEntityRow<TC>, TIR
            where TC : unmanaged, IEntityComponent, INeedEGID
            where TIR : class, IIndexableRow<TIK, TIC>
            where TIK : unmanaged
            where TIC : unmanaged, IIndexableComponent<TIK>
        {
            query.Item2.Intersect<IndexQuery<TIR, TIK, TIC>, TIR>(query.Item1, index.Query(key));
        }
    }
}