using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // supports Covariance
    public interface IEntityTable<out TRow> : IEntityTable where TRow : class, IEntityRow
    {
        internal ExclusiveGroup Group { get; }
        internal int GroupRange { get; }

        internal LocalFasterReadOnlyList<Table.PrimaryKeyInfo> PrimaryKeys { get; }

        internal EntityInitializer Build(IEntityFactory factory, uint entityID, IEnumerable<object> implementors);
        internal void Remove(IEntityFunctions functions, uint entityID);
    }

    // supports Covariance
    public interface IEntityTables<out TRow> : IEntityTables, IEntityTablesBuilder<TRow>
        where TRow : class, IEntityRow
    {
        new IEntityTable<TRow> GetTable(int index);
    }
}