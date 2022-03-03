using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // supports Covariance
    public interface IEntityTable<out TRow> : IEntityTable where TRow : IEntityRow { }

    // supports Covariance
    public interface IEntityTables<out TRow> : IEntityTables where TRow : IEntityRow
    {
        new IEntityTable<TRow> GetTable(int index);
    }
}