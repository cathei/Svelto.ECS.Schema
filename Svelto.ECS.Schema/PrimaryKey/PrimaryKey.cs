using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Svelto.ECS.Schema.Internal
{
    /***

Primary Key Component is concept of Index that split Tables
Any Index Component or State Machine Component can be used as Primary Key Component as well. 

Since the groups must stay statically - primary key component have to give you the possible range.

Primary key also have to support partial Query.

    ***/

    public interface IPrimaryKeyComponent { }

    public interface IPrimaryKeyComponent<TKey> : IIndexableComponent<TKey>, IPrimaryKeyComponent
        where TKey : unmanaged, IEquatable<TKey>
    { }

    public class PrimaryKeyBase {}

    public class PrimaryKey<TComponent> : PrimaryKeyBase
        where TComponent : unmanaged, IPrimaryKeyComponent
    { }
}
