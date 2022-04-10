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
}

