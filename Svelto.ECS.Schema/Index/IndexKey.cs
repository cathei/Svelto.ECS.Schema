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
        // This is not IEquatable because I want to keep it simple.
        // Without verbosely override object.Equals and == operator etc.
        // But if user wants they can always implement
        public interface IKeyEquatable<T>
            where T : unmanaged, IKeyEquatable<T>
        {
            bool Equals(T other);
        }
    }

    public interface IEntityIndexKey<T> : IKeyEquatable<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        public struct Component : IEntityComponent, INeedEGID
        {
            public EGID ID { get; set; }

            public T Key { get; private set; }

            // constructors should be only called when building entity
            public Component(in T key)
            {
                ID = default;
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
}