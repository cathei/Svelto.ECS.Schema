using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IStateMachineKey<TSelf> : IKeyEquatable<TSelf>
        where TSelf : IStateMachineKey<TSelf>
    { }
}
