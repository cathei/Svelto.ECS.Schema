using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // supports Covariance
    public interface IEntityTable<out TRow> : IEntityTable where TRow : class, IEntityRow
    { }

    // supports Covariance
    public interface IEntityTables<out TRow> : IEntityTables, IEntityTablesBuilder<TRow>
        where TRow : class, IEntityRow
    {
        new IEntityTable<TRow> GetTable(int index);
    }
}