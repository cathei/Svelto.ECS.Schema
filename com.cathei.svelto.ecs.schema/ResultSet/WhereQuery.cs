using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IWhereQuery
    {
        internal void Apply(ResultSetQueryConfig config);
    }

    /// <summary>
    /// contravariance (in) for TRow
    /// </summary>
    public interface IWhereQuery<in TRow> : IWhereQuery { }
}
