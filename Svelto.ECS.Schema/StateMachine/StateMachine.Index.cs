using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class EntityStateMachine<TState, TTag>
    {
        // 'available' has to be default (0)
        internal enum TransitionState
        {
            Available,
            Aborted,
            Confirmed
        }

        // user should implement this Row Interface
        public interface IIndexedRow :
            IIndexableRow<TState, Component>, IReactiveRow<Component> { }

        public struct Component : IIndexableComponent<TState>
        {
            public EGID ID { get; set; }

            internal TransitionState transitionState;

            internal TState _state;
            public TState State => _state;

            TState IIndexableComponent<TState>.Value => _state;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                _state = state;
            }
        }

        public sealed class Index : IndexBase<IIndexedRow, TState, Component> { }

        public sealed class Memo : MemoBase<IIndexedRow, Component> { }

        IndexQuery<IIndexedRow, TState, Component> IIndexQueryable<IIndexedRow, TState, Component>.Query(in TState key)
            => Config.Index.Query(key);

        IEntityIndex IEntityStateMachine.Index => Config.Index;
    }
}
