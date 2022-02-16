using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    internal interface IEntitySchemaElement { }

    internal interface IEntitySchemaTable : IEntitySchemaElement
    {
        ref readonly ExclusiveGroup ExclusiveGroup { get; }
        int Range { get; }
    }

    internal interface IEntitySchemaIndex : IEntitySchemaElement
    {
        RefWrapperType KeyType { get; }
        int IndexerId { get; }
        IEngine CreateEngine(SchemaContext context);
    }

    internal interface IEntitySchemaPartition : IEntitySchemaElement
    {
        Type ShardType { get; }
        int Range { get; }
        object GetShard(int index);
    }

    public interface IEntityIndexKey<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        // This is not IEquatable because I want to keep it simple.
        // Without verbosely override object.Equals and == operator etc.
        // But if user wants they can always implement
        bool Equals(T other);
    }

    public interface IEntityShard { }

    public interface IEntitySchema { }
}
