using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IStateMachineComponent : IIndexableComponent { }

    public interface IStateMachineComponent<TState> :
            IStateMachineComponent, IIndexableComponent<TState>
        where TState : unmanaged, IEquatable<TState>
    {
        TState IIndexableComponent<TState>.key { get => state; set => state = value; }
        TState state { get; set; }
    }
}
