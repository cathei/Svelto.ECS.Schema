using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    public abstract partial class StateMachine<TParam, TState>
        where TParam : unmanaged, IEntityComponent
        where TState : unmanaged, IEntityIndexKey<TState>
    {
        protected delegate void Callback(ref TParam param);

        internal readonly struct TransitionData
        {
            public readonly IEntityIndexKey<TState>.Wrapper next;
            public readonly Func<TParam, bool> condition;
        }

        internal struct StateData
        {
            public FasterList<TransitionData> transitions;
            public Action onEnter;
            public Action onExit;
        }

        internal FasterDictionary<IEntityIndexKey<TState>.Wrapper, StateData> states;

        protected StateMachine()
        {
            states = new FasterDictionary<Internal.IKeyEquatable<TState>.Wrapper, StateData>();
        }

        protected void AddState(in TState state)
        {

        }

        protected void AddTransition(in TState from, in TState to, Func<TParam, bool> condition)
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