using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IPrimaryKeyComponent { }

    public interface IPrimaryKeyRow : IEntityRow { }
}

namespace Svelto.ECS.Schema
{
    public interface IPrimaryKeyComponent<TKey> : IIndexableComponent<TKey>, IPrimaryKeyComponent
        where TKey : unmanaged, IEquatable<TKey>
    { }

    public interface IPrimaryKeyRow<TComponent> : IPrimaryKeyRow
        where TComponent : unmanaged, IPrimaryKeyComponent
    { }
}
