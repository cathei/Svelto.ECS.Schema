using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    internal class SchemaMetadata
    {
        internal abstract class Node
        {
            public PartitionNode parent;
            public IEntitySchemaElement element;
        }

        internal class TableNode : Node
        {
            public ExclusiveGroup group;
            public int groupSize;
        }

        internal class IndexNode : Node
        {
            public Type keyType;
            public int indexerStartIndex;
        }

        internal class PartitionNode : Node
        {
            public int groupSize;

            public FasterList<TableNode> tables;
            public FasterList<IndexNode> indexers;
            public FasterList<PartitionNode> partitions;
        }

        internal readonly PartitionNode root = new PartitionNode();

        internal readonly FasterDictionary<ExclusiveGroupStruct, TableNode> groupToTable;
        internal readonly FasterDictionary<RefWrapperType, EntitySchemaIndex> indexersToGenerateEngine;

        private static readonly Type ElementBaseType = typeof(EntitySchemaElement);
        private static readonly Type GenericTableType = typeof(Table<>);
        private static readonly Type GenericIndexType = typeof(Index<>);
        private static readonly Type GenericPartitionType = typeof(Partition<>);

        internal int indexerCount = 0;

        internal SchemaMetadata(Type schemaType)
        {
            groupToTable = new FasterDictionary<ExclusiveGroupStruct, TableNode>();
            indexersToGenerateEngine = new FasterDictionary<RefWrapperType, EntitySchemaIndex>();

            root = new PartitionNode
            {
                element = null,
                groupSize = 1
            };

            BuildTree(root, schemaType);
        }

        private void BuildTree(PartitionNode node, Type type)
        {
            foreach (var fieldInfo in GetStaticElementFields(type))
            {
                var genericType = fieldInfo.FieldType.GetGenericTypeDefinition();

                if (genericType == GenericTableType)
                {
                    if (node.tables == null)
                        node.tables = new FasterList<TableNode>();

                    var element = (EntitySchemaElement)fieldInfo.GetValue(null);

                    element.metadata = this;
                    element.siblingOrder = node.tables.count;

                    ushort groupSize = (ushort)(node.groupSize * element.range);

                    var child = new TableNode
                    {
                        parent = node,
                        element = element,
                        group = new ExclusiveGroup(groupSize),
                        groupSize = groupSize
                    };

                    node.tables.Add(child);

                    RegisterTable(child);
                }
                else if (genericType == GenericIndexType)
                {
                    if (node.indexers == null)
                        node.indexers = new FasterList<IndexNode>();

                    var element = (EntitySchemaElement)fieldInfo.GetValue(null);

                    element.metadata = this;
                    element.siblingOrder = node.indexers.count;

                    int indexerStartIndex = indexerCount;
                    indexerCount += node.groupSize;

                    // Index<T>
                    var innerType = fieldInfo.FieldType.GetGenericArguments()[0];

                    var child = new IndexNode
                    {
                        parent = node,
                        element = element,
                        indexerStartIndex = indexerStartIndex,
                        keyType = innerType,
                    };

                    node.indexers.Add(child);

                    RegisterIndexer(child);
                }
                else if (genericType == GenericPartitionType)
                {
                    if (node.partitions == null)
                        node.partitions = new FasterList<PartitionNode>();

                    var element = (EntitySchemaElement)fieldInfo.GetValue(null);

                    element.metadata = this;
                    element.siblingOrder = node.partitions.count;

                    var child = new PartitionNode
                    {
                        parent = node,
                        element = element,
                        groupSize = node.groupSize * element.range
                    };

                    node.partitions.Add(child);

                    // Partition<T>
                    var innerType = fieldInfo.FieldType.GetGenericArguments()[0];

                    BuildTree(child, innerType);
                }
                else
                {
                    throw new ECSException($"Unknown type detected in schema: {genericType}");
                }
            }
        }

        private void RegisterTable(TableNode node)
        {
            // register all possible groups
            for (ushort i = 0; i < node.groupSize; ++i)
            {
                groupToTable[node.group + i] = node;
            }

            // GroupHashMap is internal class of Svelto.ECS at the time
            // GroupHashMap.RegisterGroup(group, UniqueName);
        }

        private void RegisterIndexer(IndexNode node)
        {
            var typeRef = new RefWrapperType(node.keyType);

            if (indexersToGenerateEngine.ContainsKey(typeRef))
                return;

            indexersToGenerateEngine[typeRef] = (EntitySchemaIndex)node.element;
        }

        private static IEnumerable<FieldInfo> GetStaticElementFields(Type type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var elementFieldInfos = fieldInfos.Where(x => ElementBaseType.IsAssignableFrom(x.FieldType.BaseType));

            // the order should be deterministic and GetFields doesn't guarantee order
            // can this be affected by obfuscation?
            return elementFieldInfos.OrderBy(x => x.Name);
        }
    }
}
