using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;
using Svelto.ObjectPool;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct SelectFromQueryResult<TResult>
        where TResult : struct
    {
        public readonly TResult set;
        public readonly ExclusiveGroupStruct group;
        public readonly MultiIndexedIndices indices;

        public readonly EGIDBuilder egid => new(indices._entityIDs, group);

        public SelectFromQueryResult(TResult set,
            in ExclusiveGroupStruct group, in MultiIndexedIndices indices)
        {
            this.set = set;
            this.group = group;
            this.indices = indices;
        }
    }
}