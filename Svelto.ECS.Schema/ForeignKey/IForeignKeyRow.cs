using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public struct Referenceable<TComponent> : IEntityComponent
        where TComponent : unmanaged, IForeignKeyComponent
    { }
}

namespace Svelto.ECS.Schema
{
    public interface IReferenceableRow<TComponent> : IReactiveRow<Referenceable<TComponent>>
        where TComponent : unmanaged, IForeignKeyComponent
    { }

    public interface IForeignKeyRow<TComponent> : IReactiveRow<TComponent>
        where TComponent : unmanaged, IForeignKeyComponent
    { }
}
