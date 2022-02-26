using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public partial class StateMachine<TState>
    {
        // 'available' has to be default (0)
        internal const int TransitionAvailable = 0;
        internal const int TransitionAborted = 1;

        // TransitionConfimed + transition index
        internal const int TransitionConfimed = 2;

        public struct Component : IIndexedComponent<TState>
        {
            public EGID ID { get; set; }

            internal int nextTransition;

            public TState State { get; private set; }

            TState IIndexedComponent<TState>.Key => State;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                State = state;
            }

            internal void Update(IndexesDB indexesDB, in TState state)
            {
                TState oldState = State;
                State = state;

                // we can directly update our state machine indexer
                // since it is not attached to any node
                indexesDB.UpdateFilters(Index._indexerId, ref this, oldState, state);

                // and, propagate to others indexers in schema (is this even necessary?)
                indexesDB.NotifyKeyUpdate(ref this, oldState, state);
            }
        }

        // this will manage filters for state machine
        // it is static member here so user can easily access :)
        public static StateMachineIndex Index = new StateMachineIndex();

        public sealed class StateMachineIndex : IndexBase<TState, Component>
        {
        }
    }
}
