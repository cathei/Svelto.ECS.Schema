using System;
using System.Collections;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IEntityIndex<TComponent> :
            IWhereQueryable<IIndexableRow<TComponent>, TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }
}

