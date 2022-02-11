using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public interface IEntitySchemaElement { }

    public interface IEntityIndexKey
    {
        int Key { get; }
    }

    public interface IEntityShard
    {
        ShardOffset Offset { get; set; }
    }

    public abstract class EntitySchemaElement : IEntitySchemaElement
    {
        internal SchemaMetadata metadata;
        internal int siblingOrder;
        internal int range = 1;
    }

    public abstract class EntitySchemaIndex : EntitySchemaElement
    {
        internal abstract IEngine CreateEngine(SchemaContext context);
    }

    public struct ShardOffset
    {
        internal SchemaMetadata.PartitionNode node;
        internal int index;
    }
}