using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public partial class StateMachine<TKey>
    {
        // 'available' has to be default (0)
        internal enum TransitionState
        {
            Available,
            Aborted,
            Confirmed
        }

        public interface IIndexedRow :
            IIndexableRow<Component>, ISelectorRow<Component> { }

        public struct Component : IIndexableComponent<TKey>
        {
            public EGID ID { get; set; }

            internal TKey _key;

            public TKey Key => _key;

            // constructors should be only called when building entity
            public Component(in TKey state) : this()
            {
                _key = state;
            }
        }

        protected internal sealed class Index : IndexBase<IIndexedRow, TKey, Component> { }

        protected internal sealed class Memo : MemoBase<IIndexedRow, Component> { }

        IndexQuery<IIndexedRow, TKey> IIndexQueryable<IIndexedRow, TKey>.Query(in TKey key)
            => Config._index.Query(key);

        IEntityIndex IEntityStateMachine.Index => Config._index;
    }
}
