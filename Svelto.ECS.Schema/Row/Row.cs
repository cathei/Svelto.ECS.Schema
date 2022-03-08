using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // common parent of all rows
    public interface IEntityRow {}

    public interface IEntityRow<T> : IEntityRow
        where T : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T>(),
        };
    }

    public interface IQueryableRow<T> : IEntityRow
        where T : struct, IResultSet
    { }

    public interface IReactiveRow<TComponent> : IEntityRow<TComponent>
        where TComponent : struct, IEntityComponent
    { }
}
