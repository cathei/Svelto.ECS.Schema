using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // contravariance (in) for TRow, for type check
    public interface IPrimaryKeyProvider<in TRow> : IPrimaryKeyDefinition
        where TRow : class, IEntityRow
    { }

    // contravariance (in) for TRow, for type check
    public interface IPrimaryKeyQueryable<in TRow, TComponent>
    {
        public int PrimaryKeyID { get; }
        public Delegate KeyToIndex { get; }
    }
}

namespace Svelto.ECS.Schema
{
    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IPrimaryKey<in TRow> { }
}

