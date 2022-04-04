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
        internal readonly FasterList<IEntityTable> tables;
        internal readonly FasterList<IEntityIndex> indexers;

        internal readonly FasterDictionary<ExclusiveGroupStruct, IEntityTable> groupToTable;

        internal readonly FasterDictionary<RefWrapperType, IEntityIndex> indexersToGenerateEngine;
        internal readonly FasterDictionary<RefWrapperType, IEntityStateMachine> stateMachinesToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(ISchemaDefinition);

        internal SchemaMetadata(EntitySchema schema)
        {
            tables = new FasterList<IEntityTable>();
            indexers = new FasterList<IEntityIndex>();

            groupToTable = new FasterDictionary<ExclusiveGroupStruct, IEntityTable>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, IEntityIndex>();
            stateMachinesToGenerateEngine = new FasterDictionary<RefWrapperType, IEntityStateMachine>();

            GenerateChildren(schema, schema.GetType().FullName);
        }

        private void GenerateChildren(EntitySchema schema, string name)
        {
            foreach (var fieldInfo in GetSchemaElementFields(schema.GetType()))
            {
                var element = fieldInfo.GetValue(schema);

                if (element == null)
                    throw new ECSException("Schema element must not be null!");

                switch (element)
                {
                    case Table table:
                        RegisterTable(table, $"{name}.{fieldInfo.Name}");
                        break;

                    case IEntityIndex indexer:
                        RegisterIndexer(indexer);
                        break;

                    case IEntityStateMachine stateMachine:
                        RegisterStateMachine(stateMachine);
                        break;

                    case IEntityMemo memo:
                    case IEntityPrimaryKey pk:
                        break;

                    // case IEntitySchema schema:
                    //     GenerateChildren(new ShardNode(node), element, $"{name}.{fieldInfo.Name}");
                    //     break;

                    default:
                        throw new ECSException($"Unknown type detected in schema: {fieldInfo.FieldType.Name} {fieldInfo.Name}");
                }
            }
        }

        private void RegisterTable(Table table, string name)
        {
            table.Name = name;

            ushort groupRange = 1;

            var pks = table.primaryKeys.GetValues(out var pkCount);

            for (int p = 0; p < pkCount; ++p)
            {
                groupRange *= pks[p].PossibleKeyCount;
            }

            // group 0 reserved as build group
            if (table.primaryKeys.count > 0)
                groupRange++;

            table.group = new ExclusiveGroup(groupRange);
            table.groupRange = groupRange;

            tables.Add(table);

            for (uint i = 0; i < groupRange; ++i)
            {
                var group = table.group + i;

                groupToTable[group] = table;

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

        private void RegisterIndexer(IEntityIndex indexer)
        {
            indexers.Add(indexer);

            var componentType = indexer.ComponentType;

            if (indexersToGenerateEngine.ContainsKey(componentType))
                return;

            indexersToGenerateEngine[componentType] = indexer;
        }

        private void RegisterStateMachine(IEntityStateMachine stateMachine)
        {
            indexers.Add(stateMachine.Index);

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
