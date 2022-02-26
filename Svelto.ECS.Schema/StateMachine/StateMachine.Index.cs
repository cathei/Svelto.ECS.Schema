using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState, TUnique>
    {
        // 'available' has to be default (0)
        internal enum TransitionState
        {
            Available,
            Aborted,
            Confirmed
        }

        public struct Component : IIndexedComponent<TState>
        {
            public EGID ID { get; set; }

            internal TransitionState transitionState;

            private TState _state;
            public TState State => _state;

            TState IIndexedComponent<TState>.Value => _state;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                _state = state;
            }

            internal void Update(IndexesDB indexesDB, in TState state)
            {
                var oldState = _state;
                _state = state;

                // propagate to fsm index and others indexers in schema
                indexesDB.NotifyKeyUpdate(ref this, oldState, _state);
            }
        }

        public sealed class Index : IndexBase<TState, Component>
        {
        }

        ISchemaDefinitionIndex IEntityStateMachine.Index => Config.Index;

        public IndexQuery<TState, Component> Query(TState state) => Config.Index.Query(state);
    }
}
