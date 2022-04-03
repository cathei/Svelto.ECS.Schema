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

    public interface IReactiveRow<TComponent> : IEntityRow
        where TComponent : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<TComponent>(),
        };
    }

    public interface IQueryableRow<T> : IEntityRow
        where T : struct, IResultSet
    { }
}
