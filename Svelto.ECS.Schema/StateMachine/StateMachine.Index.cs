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
        public interface IRow : Component.IRow, IMemorableRow { }

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
                indexedDB.NotifyKeyUpdate(ref this, oldState, _state);
            }

            // this should not directly used by user
            public interface IRow : IIndexableRow<TState, Component> { }
        }

        public sealed class Index : IndexBase<IRow, TState, Component> { }

        IndexQuery<TState, Component> IIndexQueryable<IRow, TState, Component>.Query(in TState key)
            => Config.Index.Query(key);

        ISchemaDefinitionIndex IEntityStateMachine.Index => Config.Index;
    }
}
