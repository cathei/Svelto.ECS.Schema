using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    internal static class SchemaMetadata<T>
        where T : class, IEntitySchema
    {
        public static readonly SchemaMetadata Instance = new SchemaMetadata(typeof(T));
    }

    internal sealed class SchemaMetadata
    {
        internal class Node
        {
            public Node parent;
            public FasterList<IEntitySchemaIndex> indexers;
        }

        internal readonly Node root = new Node();

        internal readonly FasterDictionary<ExclusiveGroupStruct, Node> groupToParentPartition;
        internal readonly FasterDictionary<RefWrapperType, IEntitySchemaIndex> indexersToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(IEntitySchemaElement);
        private static readonly Type GenericTableType = typeof(Table<>);
        private static readonly Type GenericIndexType = typeof(Index<>);
        private static readonly Type GenericPartitionType = typeof(Partition<>);

        internal int indexerCount = 0;

        internal SchemaMetadata(Type schemaType)
        {
            groupToParentPartition = new FasterDictionary<ExclusiveGroupStruct, Node>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, IEntitySchemaIndex>();

            root = new Node();

            foreach (var fieldInfo in GetSchemaElementFields(schemaType))
            {
                RegisterChild(root, fieldInfo, null);
            }
        }

        private void RegisterChild(Node node, FieldInfo fieldInfo, object instance)
        {
            if (fieldInfo.IsStatic)
            {
                if (instance != null)
                    throw new ECSException($"Static field {fieldInfo.Name} in IEntityShard is not allowed");
            }
            else
            {
                if (instance == null)
                    throw new ECSException($"Non-static field {fieldInfo.Name} in IEntitySchema is not allowed");
            }

            var genericType = fieldInfo.FieldType.GetGenericTypeDefinition();

            if (genericType == GenericTableType)
            {
                var element = (IEntitySchemaTable)fieldInfo.GetValue(instance);

                RegisterTable(node, element);
            }
            else if (genericType == GenericIndexType)
            {
                if (node.indexers == null)
                    node.indexers = new FasterList<IEntitySchemaIndex>();

                var element = (IEntitySchemaIndex)fieldInfo.GetValue(instance);
                node.indexers.Add(element);

                RegisterIndexer(element);
            }
            else if (genericType == GenericPartitionType)
            {
                var element = (IEntitySchemaPartition)fieldInfo.GetValue(null);

                var shardType = element.ShardType;

                for (int i = 0; i < element.Range; ++i)
                {
                    var child = new Node { parent = node };

                    foreach (var shardFieldInfo in GetSchemaElementFields(shardType))
                        RegisterChild(child, shardFieldInfo, element.GetShard(i));
                }
            }
            else
            {
                throw new ECSException($"Unknown type detected in schema: {genericType}");
            }
        }

        private void RegisterTable(Node parent, IEntitySchemaTable element)
        {
            // register all possible groups
            for (ushort i = 0; i < element.Range; ++i)
            {
                groupToParentPartition[element.ExclusiveGroup + i] = parent;
            }

            // GroupHashMap is internal class of Svelto.ECS at the time
            // GroupHashMap.RegisterGroup(group, UniqueName);
        }

        private void RegisterIndexer(IEntitySchemaIndex element)
        {
            var keyType = element.KeyType;

            if (indexersToGenerateEngine.ContainsKey(keyType))
                return;

            indexersToGenerateEngine[keyType] = element;
        }

        private static IEnumerable<FieldInfo> GetSchemaElementFields(Type type)
        {
            // we get all the fields first so we can warn if user is not using proper pattern
            var fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfos.Where(x => ElementBaseType.IsAssignableFrom(x.FieldType.BaseType));
        }
    }
}
