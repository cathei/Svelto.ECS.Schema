using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IEntityRow {}

    // We only have 4 variant of IEntityRow becase that is the most we can query
    // I might write code to fetch more from EntitiesDB
    public interface IEntityRow<T1> : IEntityRow
        where T1 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
        };
    }

    public interface IEntityRow<T1, T2> : IEntityRow
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        internal static IComponentBuilder[] componentBuilders = new IComponentBuilder[] {
            new ComponentBuilder<T1>(),
            new ComponentBuilder<T2>(),
        };
    }

    public interface IEntityRow<T1, T2, T3> : IEntityRow
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

    public interface IEntityRow<T1, T2, T3, T4> : IEntityRow
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

    public abstract class EntityRow<TSelf> where TSelf : EntityRow<TSelf>
    {
        public class Table
        {

        }
    }
}