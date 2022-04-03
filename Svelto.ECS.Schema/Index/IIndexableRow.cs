using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public struct IndexableResultSet<T> : IResultSet<T, EGIDComponent>
        where T : unmanaged, IEntityComponent
    {
        public NB<T> component;
        public NB<EGIDComponent> egid;

        public void Init(in EntityCollection<T, EGIDComponent> buffers)
        {
            (component, egid, _) = buffers;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public interface IIndexableRow<TComponent> :
            IReactiveRow<TComponent>,
            IQueryableRow<IndexableResultSet<TComponent>>
        where TComponent : unmanaged, IEntityComponent
    { }
}
