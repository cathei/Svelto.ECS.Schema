using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    internal sealed partial class SchemaMetadata
    {
        internal class TableNode
        {
            public ShardNode parent;
            public ISchemaDefinitionTable table;
        }

        internal class ShardNode
        {
            public ShardNode parent;
            public FasterList<ISchemaDefinitionIndex> indexers;

            public ShardNode(ShardNode parent)
            {
                this.parent = parent;
            }
        }

        internal readonly ShardNode root;

        internal readonly FasterDictionary<ExclusiveGroupStruct, TableNode> groupToTable;
        internal readonly FasterDictionary<RefWrapperType, ISchemaDefinitionIndex> indexersToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(ISchemaDefinition);

        internal SchemaMetadata(IEntitySchema schema)
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, TableNode>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, ISchemaDefinitionIndex>();

            root = new ShardNode(null);
            GenerateChildren(root, schema, schema.GetType().FullName);
        }

        private void GenerateChildren(ShardNode node, object instance, string name)
        {
            foreach (var fieldInfo in GetSchemaElementFields(instance.GetType()))
            {
                var element = fieldInfo.GetValue(instance);

                if (element == null)
                    throw new ECSException("Schema element must not be null!");

                switch (element)
                {
                    case ISchemaDefinitionTable table:
                        RegisterTable(node, table, $"{name}.{fieldInfo.Name}");
                        break;

                    case ISchemaDefinitionRangedTable rangedTable:
                        for (int i = 0; i < rangedTable.Range; ++i)
                            RegisterTable(node, rangedTable.GetTable(i), $"{name}.{fieldInfo.Name}.{i}");
                        break;

                    case ISchemaDefinitionIndex indexer:
                        RegisterIndexer(node, indexer);
                        break;

                    case IEntitySchema schema:
                        GenerateChildren(new ShardNode(node), element, $"{name}.{fieldInfo.Name}");
                        break;

                    case ISchemaDefinitionRangedSchema rangedSchema:
                        for (int i = 0; i < rangedSchema.Range; ++i)
                            GenerateChildren(new ShardNode(node), rangedSchema.GetSchema(i), $"{name}.{fieldInfo.Name}.{i}");
                        break;

                    case ISchemaDefinitionMemo memo:
                    // case ISchemaDefinitionStateMachine stateMachine:
                        break;

                    default:
                        throw new ECSException($"Unknown type detected in schema: {fieldInfo.FieldType.Name} {fieldInfo.Name}");
                }
            }
        }

        private void RegisterTable(ShardNode parent, ISchemaDefinitionTable table, string name)
        {
            groupToTable[table.ExclusiveGroup] = new TableNode
            {
                parent = parent,
                table = table
            };

            // ok we have to set three internal Svelto Dictionary here to support serialization
            // GroupHashMap
            GroupHashMapRegisterGroup?.Invoke(null, new object[] { table.ExclusiveGroup, name });
            // GroupNamesMap.idToName
            GroupNamesMapIdToName?.Add(table.ExclusiveGroup, $"{name} {table.ExclusiveGroup.id})");
            // ExclusiveGroup._knownGroups
            ExclusiveGroupKnownGroups?.Add(name, table.ExclusiveGroup);
        }

        private void RegisterIndexer(ShardNode node, ISchemaDefinitionIndex indexer)
        {
            node.indexers ??= new FasterList<ISchemaDefinitionIndex>();
            node.indexers.Add(indexer);

            var keyType = indexer.KeyType;

            if (indexersToGenerateEngine.ContainsKey(keyType))
                return;

            indexersToGenerateEngine[keyType] = indexer;
        }

        private static IEnumerable<FieldInfo> GetSchemaElementFields(Type type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfos.Where(x => ElementBaseType.IsAssignableFrom(x.FieldType));
        }
    }
}
