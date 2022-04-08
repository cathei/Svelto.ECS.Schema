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
    public readonly ref struct EGIDBuilder
    {
        private readonly NativeEntityIDs _entityIDs;
        internal readonly ExclusiveGroupStruct _group;

        public EGIDBuilder(NativeEntityIDs entityIDs, ExclusiveGroupStruct group)
        {
            _entityIDs = entityIDs;
            _group = group;
        }

        public EGID this[int index] => new(_entityIDs[index], _group);
        public EGID this[uint index] => new(_entityIDs[index], _group);
    }
}