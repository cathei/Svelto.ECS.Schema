using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public interface IForeignKeyComponent : IEntityComponent
    {
        public EntityReference reference { get; set; }
    }
}
