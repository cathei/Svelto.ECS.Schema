using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
}

namespace Svelto.ECS.Schema
{
    public interface IIndexableRow<TComponent> :
            IReactiveRow<TComponent>, IKeyedRow<TComponent>
        where TComponent : unmanaged, IKeyComponent
    { }
}
