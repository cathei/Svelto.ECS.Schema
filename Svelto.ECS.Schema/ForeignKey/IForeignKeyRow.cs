using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IForeignKeyRow<TComponent> : IQueryableRow<ResultSet<TComponent>>
        where TComponent : unmanaged, IForeignKeyComponent
    { }
}
