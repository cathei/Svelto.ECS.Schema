using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // this includes T component, but does not impact for your selector row
    public interface IEntityRow<T> : IEntityRow
        where T : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T>(),
        };
    }
}

namespace Svelto.ECS.Schema
{
    // common parent of all rows
    public interface IEntityRow {}

    // We only have 4 variant of ISelectorRow becase that is the most we can query
    // I might write code to fetch more from EntitiesDB
    public interface ISelectorRow<T1> : IEntityRow<T1>
        where T1 : struct, IEntityComponent
    {
        // extends IEntityRow<T1> so no componentBuilders here
    }

    public interface ISelectorRow<T1, T2> : IEntityRow
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
            new ComponentBuilder<T2>(),
        };
    }

    public interface ISelectorRow<T1, T2, T3> : IEntityRow
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
            new ComponentBuilder<T2>(),
            new ComponentBuilder<T3>(),
        };
    }

    public interface ISelectorRow<T1, T2, T3, T4> : IEntityRow
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
        where T4 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
            new ComponentBuilder<T2>(),
            new ComponentBuilder<T3>(),
            new ComponentBuilder<T4>(),
        };
    }

    public interface IReactiveRow<TComponent> : IEntityRow<TComponent>
        where TComponent : struct, IEntityComponent
    { }
}
