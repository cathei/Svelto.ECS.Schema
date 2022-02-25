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
        void Remove(IEntityFunctions functions, uint entityID);
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

    internal interface IEntitySchemaMemo : IEntitySchemaElement { }

    public interface IEntitySchema { }
}
