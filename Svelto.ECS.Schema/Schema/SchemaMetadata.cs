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
        internal readonly FasterDictionary<RefWrapperType, IEntityStateMachine> stateMachinesToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(ISchemaDefinition);

        internal SchemaMetadata(IEntitySchema schema)
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, TableNode>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, IEntityIndex>();
            stateMachinesToGenerateEngine = new FasterDictionary<RefWrapperType, IEntityStateMachine>();

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
                    case Table table:
                        RegisterTable(node, table, $"{name}.{fieldInfo.Name}");
                        break;

                    case IEntitySchema schema:
                        GenerateChildren(new ShardNode(node), element, $"{name}.{fieldInfo.Name}");
                        break;

                    case IEntityIndex indexer:
                        RegisterIndexer(node, indexer);
                        break;

                    case IEntityStateMachine stateMachine:
                        RegisterStateMachine(node, stateMachine);
                        break;

                    case IEntityPrimaryKey pk:
                    case ISchemaDefinitionMemo memo:
                        break;

                    default:
                        throw new ECSException($"Unknown type detected in schema: {fieldInfo.FieldType.Name} {fieldInfo.Name}");
                }
            }
        }

        private void RegisterTable(ShardNode parent, Table table, string name)
        {
            table.Name = name;

            ushort groupRange = 1;

            foreach (var pk in table.primaryKeys)
            {
                groupRange *= pk.PossibleKeyCount;
            }

            // group 0 reserved as build group
            if (table.primaryKeys.count > 0)
                groupRange++;

            table.group = new ExclusiveGroup(groupRange);
            table.groupRange = groupRange;

            for (uint i = 0; i < groupRange; ++i)
            {
                var group = table.group + i;

                groupToTable[group] = new TableNode
                {
                    parent = parent,
                    table = table
                };

                var groupName = $"{name}-({i+1}/{groupRange})";

                // ok we have to set three internal Svelto Dictionary here to support serialization
                // GroupHashMap
                // GroupHashMapRegisterGroup?.Invoke(null, new object[] { table.ExclusiveGroup, name });
                // GroupNamesMap.idToName
                GroupNamesMapIdToName?.Add(group, groupName);
                // ExclusiveGroup._knownGroups
                ExclusiveGroupKnownGroups?.Add(groupName, group);
            }
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

        private void RegisterStateMachine(ShardNode node, IEntityStateMachine stateMachine)
        {
            node.indexers ??= new FasterList<IEntityIndex>();
            node.indexers.Add(stateMachine.Index);

            var componentType = stateMachine.ComponentType;

            if (stateMachinesToGenerateEngine.ContainsKey(componentType))
                return;

            stateMachinesToGenerateEngine[componentType] = stateMachine;
        }

        private static IEnumerable<FieldInfo> GetSchemaElementFields(Type type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfos.Where(x => ElementBaseType.IsAssignableFrom(x.FieldType));
        }
    }
}
