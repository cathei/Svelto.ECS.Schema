using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public readonly partial struct Groups<T> where T : IEntityDescriptor
    {
        public readonly FasterReadOnlyList<ExclusiveGroupStruct> exclusiveGroups;

        public Groups(FasterList<ExclusiveGroupStruct> exclusiveGroups)
        {
            this.exclusiveGroups = exclusiveGroups;
        }

        public static implicit operator FasterReadOnlyList<ExclusiveGroupStruct>(Groups<T> groups) => groups.exclusiveGroups;
        public static implicit operator LocalFasterReadOnlyList<ExclusiveGroupStruct>(Groups<T> groups) => groups.exclusiveGroups;
    }
}