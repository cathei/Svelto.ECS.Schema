using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TParam, TState>
    {
        public struct Component : IIndexedComponent<TState>
        {
            public EGID ID { get; set; }

            public TState State { get; private set; }

            TState IIndexedComponent<TState>.Key => State;

            public TParam param;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                State = state;
            }

            internal void Update(IndexesDB indexesDB, in TState state)
            {
                TState oldState = State;
                State = state;
                indexesDB.NotifyKeyUpdate(ref this, oldState, state);
            }
        }

        public sealed class Index : IndexBase<TState, Component>
        {
        }
    }
}
