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
        where T : unmanaged, IEntityIndexKey
    {
        public EGID ID { get; set; }

        public T Content { get; private set; }

        public int Key => Content.Key;

        // constructors should be only called when building entity
        public Indexed(in T content)
        {
            ID = default;
            Content = content;
        }

        public void Update(SchemaContext context, in T content)
        {
            int oldKey = Key;
            Content = content;
            context.NotifyKeyUpdate(ref this, oldKey);
        }
    }
}