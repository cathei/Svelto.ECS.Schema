using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IPrimaryKeyRow : IQueryableRow<ResultSet<RowIdentityComponent>>{ }
}

namespace Svelto.ECS.Schema
{
    public interface IPrimaryKeyRow<TComponent> : IPrimaryKeyRow, IEntityRow<TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }
}
