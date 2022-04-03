using System;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IStateMachineComponent : IKeyComponent { }

    public interface IStateMachineComponent<TState> :
            IStateMachineComponent, IKeyComponent<TState>
        where TState : unmanaged, IEquatable<TState>
    { }
}
