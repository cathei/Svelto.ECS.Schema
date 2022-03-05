using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{
    public partial class StateMachine<TState>
    {
        // 'available' has to be default (0)
        internal enum TransitionState
        {
            Available,
            Aborted,
            Confirmed
        }

        protected internal interface IIndexedRow : IIndexableRow<Component> { }

        public struct Component : IIndexableComponent<TState>
        {
            public EGID ID { get; set; }

            internal TState _state;

            public TState Key => _state;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                _state = state;
            }
        }

        protected internal sealed class Index : IndexBase<IIndexedRow, TState, Component> { }

        protected internal sealed class Memo : MemoBase<IIndexedRow, Component> { }

        IndexQuery<IIndexedRow, TState> IIndexQueryable<IIndexedRow, TState>.Query(in TState key)
            => Config._index.Query(key);

        IEntityIndex IEntityStateMachine.Index => Config._index;
    }
}
