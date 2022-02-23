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

        void Remove(IEntityFunctions functions, EGID egid);
    }

    internal interface IEntitySchemaIndex : IEntitySchemaElement
    {
        RefWrapperType KeyType { get; }
        int IndexerId { get; }
        IEngine CreateEngine(IndexesDB indexesDB);
    }

    internal interface IEntitySchemaShard : IEntitySchemaElement
    {
        Type InnerType { get; }
        int Range { get; }
        object GetSchema(int index);
    }

    public interface IEntityIndexKey<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        // This is not IEquatable because I want to keep it simple.
        // Without verbosely override object.Equals and == operator etc.
        // But if user wants they can always implement
        bool Equals(T other);
    }

    public interface IEntitySchema { }
}
