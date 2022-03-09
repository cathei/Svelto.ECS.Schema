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
            public IEntityTable table;
        }

        internal class ShardNode
        {
            public ShardNode parent;
            public FasterList<IEntityIndex> indexers;

            public ShardNode(ShardNode parent)
            {
                this.parent = parent;
            }
        }

        internal readonly ShardNode root;

        internal readonly FasterDictionary<ExclusiveGroupStruct, TableNode> groupToTable;
        internal readonly FasterDictionary<RefWrapperType, IEntityIndex> indexersToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(ISchemaDefinition);

        internal SchemaMetadata(IEntitySchema schema)
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, TableNode>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, IEntityIndex>();

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
                    case IEntityTable table:
                        RegisterTable(node, table, $"{name}.{fieldInfo.Name}");
                        break;

                    case IEntityTables rangedTable:
                        rangedTable.Name = $"{name}.{fieldInfo.Name}";

                        // metadata should not include combined tables
                        if (rangedTable.IsCombined)
                            break;

                        for (int i = 0; i < rangedTable.Range; ++i)
                            RegisterTable(node, rangedTable.GetTable(i), $"{name}.{fieldInfo.Name}.{i}");
                        break;

                    case IEntityIndex indexer:
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

        private void RegisterTable(ShardNode parent, IEntityTable table, string name)
        {
            table.Name = name;

            groupToTable[table.ExclusiveGroup] = new TableNode
            {
                parent = parent,
                table = table
            };

            // ok we have to set three internal Svelto Dictionary here to support serialization
            // GroupHashMap
            // GroupHashMapRegisterGroup?.Invoke(null, new object[] { table.ExclusiveGroup, name });
            // GroupNamesMap.idToName
            GroupNamesMapIdToName?.Add(table.ExclusiveGroup, $"{name}-{table.ExclusiveGroup.id}");
            // ExclusiveGroup._knownGroups
            ExclusiveGroupKnownGroups?.Add(name, table.ExclusiveGroup);
        }

        private void RegisterIndexer(ShardNode node, IEntityIndex indexer)
        {
            node.indexers ??= new FasterList<IEntityIndex>();
            node.indexers.Add(indexer);

            var componentType = indexer.ComponentType;

            if (indexersToGenerateEngine.ContainsKey(componentType))
                return;

            indexersToGenerateEngine[componentType] = indexer;
        }

        private static IEnumerable<FieldInfo> GetSchemaElementFields(Type type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfos.Where(x => ElementBaseType.IsAssignableFrom(x.FieldType));
        }
    }
}
