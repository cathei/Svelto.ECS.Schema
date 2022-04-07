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
    public readonly ref struct QueryResult<TResult>
        where TResult : struct
    {
        public readonly TResult set;
        public readonly ExclusiveGroupStruct group;
        public readonly MultiIndexedIndices indices;

        public readonly EGIDBuilder egid => new(indices._entityIDs, group);

        public QueryResult(TResult set,
            in ExclusiveGroupStruct group, in MultiIndexedIndices indices)
        {
            this.set = set;
            this.group = group;
            this.indices = indices;
        }
    }

    public readonly ref struct QueryResult<TResult, TJoined, TJoinComponent>
        where TResult : struct
        where TJoined : struct
        where TJoinComponent : unmanaged, IForeignKeyComponent
    {
        public readonly TResult setA;
        public readonly TJoined setB;

        public readonly ExclusiveGroupStruct groupA;
        public readonly ExclusiveGroupStruct groupB;

        public readonly JoinedIndexedIndices<TJoinComponent> indices;

        public readonly EGIDBuilder egidA => new(indices._inner._entityIDs, groupA);

        public QueryResult(
            in TResult setA, in ExclusiveGroupStruct groupA,
            in TJoined setB, in ExclusiveGroupStruct groupB,
            in JoinedIndexedIndices<TJoinComponent> indices)
        {
            this.setA = setA;
            this.setB = setB;

            this.groupA = groupA;
            this.groupB = groupB;

            this.indices = indices;
        }
    }

    public readonly ref struct QueryResult<TResult, TJoined1, TJoined2, TJoinComponent1, TJoinComponent2>
        where TResult : struct
        where TJoined1 : struct
        where TJoined2 : struct
        where TJoinComponent1 : unmanaged, IForeignKeyComponent
        where TJoinComponent2 : unmanaged, IForeignKeyComponent
    {
        public readonly TResult setA;
        public readonly TJoined1 setB;
        public readonly TJoined2 setC;

        public readonly ExclusiveGroupStruct groupA;
        public readonly ExclusiveGroupStruct groupB;
        public readonly ExclusiveGroupStruct groupC;

        public readonly JoinedIndexedIndices<TJoinComponent1, TJoinComponent2> indices;

        public readonly EGIDBuilder egidA => new(indices._inner._inner._entityIDs, groupA);

        public QueryResult(
            in TResult setA, in ExclusiveGroupStruct groupA,
            in TJoined1 setB, in ExclusiveGroupStruct groupB,
            in TJoined2 setC, in ExclusiveGroupStruct groupC,
            in JoinedIndexedIndices<TJoinComponent1, TJoinComponent2> indices)
        {
            this.setA = setA;
            this.setB = setB;
            this.setC = setC;

            this.groupA = groupA;
            this.groupB = groupB;
            this.groupC = groupC;

            this.indices = indices;
        }
    }
}