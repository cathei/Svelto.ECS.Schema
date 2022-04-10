using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IIndexQueryable<in TRow, TComponent>
        where TRow : class, IEntityRow
        where TComponent : unmanaged, IEntityComponent
    {
        public FilterContextID IndexerID { get; }
    }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IEntityIndex<TComponent> :
            IIndexQueryable<IIndexableRow<TComponent>, TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }
}

