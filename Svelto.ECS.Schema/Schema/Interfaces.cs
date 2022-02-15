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

    public interface IEntityIndexKey<T>
        where T : unmanaged, IEntityIndexKey<T>
    {
        // This is not IEquatable because I want to keep it simple.
        // Without verbosely override object.Equals and == operator etc.
        // But if user wants they can always implement
        bool Equals(T other);
    }

    public interface IEntityShard
    {
        ShardOffset Offset { get; set; }
    }

    public readonly struct ShardOffset
    {
        internal readonly SchemaMetadata.PartitionNode node;
        internal readonly int index;

        internal ShardOffset(SchemaMetadata.PartitionNode node, int index)
        {
            this.node = node;
            this.index = index;
        }
    }
}