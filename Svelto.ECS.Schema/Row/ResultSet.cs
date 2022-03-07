namespace Svelto.ECS.Schema
{
    public interface IResultSet
    {
        // it is user's responsiblity to set count when Init() called
        public int count { get; }
    }

    public interface IResultSet<T1> : IResultSet
        where T1 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders;

        void Init(in EntityCollection<T1> buffers);
    }

    public interface IResultSet<T1, T2> : IResultSet
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders;

        void Init(in EntityCollection<T1, T2> buffers);
    }

    public interface IResultSet<T1, T2, T3> : IResultSet
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders;

        void Init(in EntityCollection<T1, T2, T3> buffers);
    }

    public interface IResultSet<T1, T2, T3, T4> : IResultSet
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
        where T4 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders;

        void Init(in EntityCollection<T1, T2, T3, T4> buffers);
    }
}