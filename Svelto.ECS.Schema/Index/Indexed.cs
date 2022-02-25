using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    namespace Internal
    {
        // technically this can be extended but it is not easy to do so
        public interface IIndexedComponent<T> : IEntityComponent
            where T : unmanaged, IKeyEquatable<T>
        {
            EGID ID { get; }
            T Key { get; }
        }
    }

    // simple implementation to force index update
    public struct Indexed<T> : IIndexedComponent<T>, INeedEGID
        where T : unmanaged, IEntityIndexKey<T>
    {
        public EGID ID { get; set; }

        public T Key { get; private set; }

        // constructors should be only called when building entity
        public Indexed(in T key) : this()
        {
            Key = key;
        }

        public void Update(IndexesDB indexesDB, in T key)
        {
            T oldKey = Key;
            Key = key;
            indexesDB.NotifyKeyUpdate(ref this, oldKey, key);
        }
    }
}