using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Svelto.ECS.Schema.Internal
{
    internal static class ResultSetHelper<T> where T : struct, IResultSet
    {
        // boxed as template
        private readonly static ThreadLocal<IResultSet> defaultBoxed = new(() => default(T));

        // some tricky thing is happening here but this should be threadsafe
        public static void Assign(out T resultSet, EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            var boxed = defaultBoxed.Value;
            boxed.LoadEntities(entitiesDB, groupID);
            resultSet = (T)boxed;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public interface IResultSet
    {
        void LoadEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID);
    }

    // We only have 4 variant of IResultSet becase that is the most we can query
    // I might write code to fetch more from EntitiesDB
    public interface IResultSet<T1> : IResultSet
        where T1 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
        };

        void Init(in EntityCollection<T1> buffers);

        void IResultSet.LoadEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            Init(entitiesDB.QueryEntities<T1>(groupID));
        }
    }

   public interface IResultSet<T1, T2> : IResultSet
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
            new ComponentBuilder<T2>(),
        };

        void Init(in EntityCollection<T1, T2> buffers);

        void IResultSet.LoadEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            Init(entitiesDB.QueryEntities<T1, T2>(groupID));
        }
    }

    public interface IResultSet<T1, T2, T3> : IResultSet
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
            new ComponentBuilder<T2>(),
            new ComponentBuilder<T3>(),
        };

        void Init(in EntityCollection<T1, T2, T3> buffers);

        void IResultSet.LoadEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            Init(entitiesDB.QueryEntities<T1, T2, T3>(groupID));
        }
    }

    public interface IResultSet<T1, T2, T3, T4> : IResultSet
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

        void Init(in EntityCollection<T1, T2, T3, T4> buffers);

        void IResultSet.LoadEntities(EntitiesDB entitiesDB, in ExclusiveGroupStruct groupID)
        {
            Init(entitiesDB.QueryEntities<T1, T2, T3, T4>(groupID));
        }
    }
}