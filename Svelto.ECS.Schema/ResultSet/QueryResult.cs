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
        public readonly ExclusiveGroupStruct group => egid._group;
        public readonly MultiIndexedIndices indices;

        public readonly EGIDBuilder egid;

        public QueryResult(TResult set,
            in ExclusiveGroupStruct group, in MultiIndexedIndices indices)
        {
            this.set = set;
            this.indices = indices;

            egid = new(indices._entityIDs, group);
        }
    }

    public readonly ref struct QueryResult<TResult, TJoined, TJoinComponent>
        where TResult : struct
        where TJoined : struct
        where TJoinComponent : unmanaged, IForeignKeyComponent
    {
        public readonly TResult setA;
        public readonly TJoined setB;

        public readonly ExclusiveGroupStruct groupA => egidA._group;
        public readonly ExclusiveGroupStruct groupB => egidB._group;

        public readonly JoinedIndexedIndices<TJoinComponent> indices;

        public readonly EGIDBuilder egidA;
        public readonly EGIDBuilder egidB;

        public QueryResult(
            in TResult setA, in ExclusiveGroupStruct groupA,
            in TJoined setB, in ExclusiveGroupStruct groupB,
            in JoinedIndexedIndices<TJoinComponent> indices, NativeEntityIDs groupBEntityIDs)
        {
            this.setA = setA;
            this.setB = setB;

            this.indices = indices;

            egidA = new(indices._inner._entityIDs, groupA);
            egidB = new(groupBEntityIDs, groupB);
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

        public readonly ExclusiveGroupStruct groupA => egidA._group;
        public readonly ExclusiveGroupStruct groupB => egidB._group;
        public readonly ExclusiveGroupStruct groupC => egidC._group;

        public readonly JoinedIndexedIndices<TJoinComponent1, TJoinComponent2> indices;

        public readonly EGIDBuilder egidA;
        public readonly EGIDBuilder egidB;
        public readonly EGIDBuilder egidC;

        public QueryResult(
            in TResult setA, in ExclusiveGroupStruct groupA,
            in TJoined1 setB, in ExclusiveGroupStruct groupB,
            in TJoined2 setC, in ExclusiveGroupStruct groupC,
            in JoinedIndexedIndices<TJoinComponent1, TJoinComponent2> indices,
            NativeEntityIDs groupBEntityIDs, NativeEntityIDs groupCEntityIDs)
        {
            this.setA = setA;
            this.setB = setB;
            this.setC = setC;

            this.indices = indices;

            egidA = new(indices._inner._inner._entityIDs, groupA);
            egidB = new(groupBEntityIDs, groupB);
            egidC = new(groupCEntityIDs, groupC);
        }
    }
}