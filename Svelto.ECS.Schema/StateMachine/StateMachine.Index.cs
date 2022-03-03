using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TTag, TState>
    {
        // 'available' has to be default (0)
        internal enum TransitionState
        {
            Available,
            Aborted,
            Confirmed
        }

        // user should implement this Row Interface
        public interface IRow : IIndexableRow<TState, Component>, IReactiveRow<IRow, Component> { }

        public struct Component : IIndexableComponent<TState>
        {
            public EGID ID { get; set; }

            internal TransitionState transitionState;

            private TState _state;
            public TState State => _state;

            TState IIndexableComponent<TState>.Value => _state;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                _state = state;
            }

            internal void Update(IndexedDB indexedDB, in TState state)
            {
                var oldState = _state;
                _state = state;

                // propagate to fsm index and others indexers in schema
                indexedDB.NotifyKeyUpdate<IRow, TState, Component>(ref this, oldState, _state);
            }
        }

        public sealed class Index : IndexBase<IRow, TState, Component> { }

        public sealed class Memo : MemoBase<IRow, Component> { }

        IndexQuery<IRow, TState, Component> IIndexQueryable<IRow, TState, Component>.Query(in TState key)
            => Config.Index.Query(key);

        ISchemaDefinitionIndex IEntityStateMachine.Index => Config.Index;
    }
}
