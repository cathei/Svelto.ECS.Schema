using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public struct ResultSet<T> : IResultSet<T>
        where T : unmanaged, IEntityComponent
    {
        public NB<T> component;
        public NativeEntityIDs entityIDs;

        public void Init(in EntityCollection<T> buffers)
        {
            (component, entityIDs, _) = buffers;
        }
    }
}

namespace Svelto.ECS.Schema
{
    public interface IIndexableRow<TComponent> : IQueryableRow<ResultSet<TComponent>>
        where TComponent : unmanaged, IEntityComponent
    { }
}
