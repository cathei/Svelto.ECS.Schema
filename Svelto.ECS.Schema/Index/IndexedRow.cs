using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IIndexableRow<TComponent> : IReactiveRow<TComponent>
        where TComponent : struct, IIndexableComponent
    { }
}
