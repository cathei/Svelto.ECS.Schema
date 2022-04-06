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
    public readonly ref struct SelectFromJoinQueryResult<TResult, TJoined, TJoinComponent>
        where TResult : struct
        where TJoinComponent : unmanaged, IForeignKeyComponent
    {
        public readonly TResult set;
        public readonly ExclusiveGroupStruct group;

        public readonly TJoined joined;
        public readonly ExclusiveGroupStruct joinedGroup;

        public readonly JoinedIndexedIndices<TJoinComponent> indices;

        public readonly EGIDBuilder egid => new(indices._inner._entityIDs, group);

        public SelectFromJoinQueryResult(
            in TResult set, in ExclusiveGroupStruct group,
            in TJoined joined, in ExclusiveGroupStruct joinedGroup,
            in JoinedIndexedIndices<TJoinComponent> indices)
        {
            this.set = set;
            this.group = group;

            this.joined = joined;
            this.joinedGroup = joinedGroup;

            this.indices = indices;
        }
    }
}