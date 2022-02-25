using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    public class StateMachine<TParam, TState>
        where TParam : unmanaged, IEntityComponent
        where TState : unmanaged, IEntityIndexKey<TState>
    {
        public void AddState(in TState state)
        {

        }

        public void AddTransition(in TState from, in TState to, Func<TParam, bool> condition)
        {

        }

        public void EnterTransition(in TState state)
        {

        }
    }
}