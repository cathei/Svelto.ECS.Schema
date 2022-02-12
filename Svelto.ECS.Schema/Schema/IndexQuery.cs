using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly struct IndexQuery
    {
        internal readonly int indexerId;
        internal readonly int key;

        internal IndexQuery(int indexerId, int key)
        {
            this.indexerId = indexerId;
            this.key = key;
        }
    }
}