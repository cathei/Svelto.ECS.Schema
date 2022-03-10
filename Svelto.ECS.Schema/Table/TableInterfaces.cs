using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // supports Covariance
    public interface IEntityTable<out TRow> : IEntityTable where TRow : class, IEntityRow
    {
        internal void Remove(IEntityFunctions functions, uint entityID);
    }

    // supports Covariance
    public interface IEntityTables<out TRow> : IEntityTables where TRow : class, IEntityRow
    {
        new IEntityTable<TRow> GetTable(int index);
    }
}