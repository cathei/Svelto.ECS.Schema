using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    internal sealed class SchemaMetadata
    {
        internal class TableNode
        {
            public ShardNode parent;
            public IEntitySchemaTable table;
        }

        internal class ShardNode
        {
            public ShardNode parent;
            public FasterList<IEntitySchemaIndex> indexers;
        }

        internal readonly ShardNode root = new ShardNode();

        internal readonly FasterDictionary<ExclusiveGroupStruct, TableNode> groupToTable;
        internal readonly FasterDictionary<RefWrapperType, IEntitySchemaIndex> indexersToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(IEntitySchemaElement);
        private static readonly Type GenericTableType = typeof(Table<>);
        private static readonly Type GenericIndexType = typeof(Index<>);
        private static readonly Type GenericShardType = typeof(Shard<>);

        internal SchemaMetadata(IEntitySchema schema)
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, TableNode>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, IEntitySchemaIndex>();

            root = new ShardNode();

            GenerateChildren(root, schema);
        }

        private void GenerateChildren(ShardNode node, object instance)
        {
            foreach (var fieldInfo in GetSchemaElementFields(instance.GetType()))
            {
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
                else if (genericType == GenericShardType)
                {
                    var element = (IEntitySchemaShard)fieldInfo.GetValue(instance);

                    var shardType = element.InnerType;

                    for (int i = 0; i < element.Range; ++i)
                    {
                        var child = new ShardNode { parent = node };

                        GenerateChildren(child, element.GetSchema(i));
                    }
                }
                else
                {
                    throw new ECSException($"Unknown type detected in schema: {genericType}");
                }
            }
        }

        private void RegisterTable(ShardNode parent, IEntitySchemaTable element)
        {
            var node = new TableNode
            {
                parent = parent,
                table = element
            };

            // register all possible groups
            for (ushort i = 0; i < element.Range; ++i)
            {
                groupToTable[element.ExclusiveGroup + i] = node;
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
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfos.Where(x => ElementBaseType.IsAssignableFrom(x.FieldType));
        }
    }
}
