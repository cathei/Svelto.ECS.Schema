using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IPrimaryKeyComponent : IEntityComponent { }

    public interface IPrimaryKeyRow : IEntityRow, IMemorableRow, IReactiveRow<RowIdentityComponent> { }
}

namespace Svelto.ECS.Schema
{
    public interface IPrimaryKeyComponent<TKey> : IIndexableComponent<TKey>, IPrimaryKeyComponent
        where TKey : unmanaged, IEquatable<TKey>
    { }

    public interface IPrimaryKeyRow<TComponent> : IPrimaryKeyRow, IEntityRow<TComponent>
        where TComponent : unmanaged, IPrimaryKeyComponent
    { }
}
