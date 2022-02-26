using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
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

        public readonly struct Key : IEntityIndexKey<Key>
        {
            internal readonly TState _state;

            public Key(TState state)
            {
                _state = state;
            }

            public bool Equals(Key other)
            {
                return Comparer.Equals(_state, other._state);
            }

            public static implicit operator Key(in TState state) => new Key(state);
            public static implicit operator TState(in Key key) => key._state;
        }

        public struct Component : IIndexedComponent<Key>, INeedEGID
        {
            public EGID ID { get; set; }

            internal TransitionState transitionState;

            public TState State => _key;

            internal Key _key;

            Key IIndexedComponent<Key>.Key => _key;

            // constructors should be only called when building entity
            public Component(in TState state) : this()
            {
                _key = state;
            }

            internal void Update(IndexesDB indexesDB, in TState state)
            {
                Key oldKey = _key;
                _key = state;

                // propagate to fsm index and others indexers in schema
                indexesDB.NotifyKeyUpdate(ref this, oldKey, _key);
            }
        }

        internal sealed class StateMachineIndex : IndexBase<Key, Component>
        {
        }

        // this will manage filters for state machine
        internal StateMachineIndex Index = new StateMachineIndex();

        ISchemaDefinitionIndex IEntityStateMachine.Index => Index;
    }
}
