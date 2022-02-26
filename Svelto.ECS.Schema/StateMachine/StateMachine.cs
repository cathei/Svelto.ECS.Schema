using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public abstract partial class StateMachine<TParam, TState> : ISchemaDefinitionStateMachine
        where TParam : unmanaged
        where TState : unmanaged, IKeyEquatable<TState>
    {
        public delegate bool Condition(in TParam param);
        public delegate void Callback(ref TParam param);

        internal readonly struct TransitionData
        {
            public readonly IKeyEquatable<TState>.Wrapper next;
            public readonly Condition condition;
        }

        internal struct StateData
        {
            public FasterList<TransitionData> transitions;
            public Action onEnter;
            public Action onExit;
        }

        internal FasterDictionary<IKeyEquatable<TState>.Wrapper, StateData> states;

        protected StateMachine()
        {
            states = new FasterDictionary<Internal.IKeyEquatable<TState>.Wrapper, StateData>();
        }

        protected void AddState(in TState state)
        {

        }

        protected void AddTransition(in TState from, in TState to, Condition condition)
        {

        }

        protected void OnEnter(in TState state, Callback callback)
        {

        }

        protected void OnExit(in TState state, Callback callback)
        {

        }
    }
}