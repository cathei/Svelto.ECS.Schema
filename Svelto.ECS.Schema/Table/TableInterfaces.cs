using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // supports Covariance
    public interface IEntityTable<out TRow> : IEntityTable where TRow : IEntityRow { }

    // supports Covariance
    public interface IEntityTables<out TRow> : IEntityTables where TRow : IEntityRow
    {
        new IEntityTable<TRow> GetTable(int index);
    }
}