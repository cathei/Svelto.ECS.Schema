using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public struct Indexed<T> : IEntityComponent, INeedEGID
        where T : unmanaged, IEntityIndexKey<T>
    {
        public EGID ID { get; set; }

        public T Key { get; private set; }

        // constructors should be only called when building entity
        public Indexed(in T key)
        {
            ID = default;
            Key = key;
        }

        public void Update(SchemaContext context, in T key)
        {
            T oldKey = Key;
            Key = key;
            context.NotifyKeyUpdate(ref this, oldKey, key);
        }
    }
}