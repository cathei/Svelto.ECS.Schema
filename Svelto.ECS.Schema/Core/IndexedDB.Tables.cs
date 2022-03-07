using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        internal readonly FasterDictionary<ExclusiveGroupStruct, IEntityTable> _groupToTable
            = new FasterDictionary<ExclusiveGroupStruct, IEntityTable>();

        internal readonly FasterDictionary<RefWrapperType, IEntityTables> _rowToTables
            = new FasterDictionary<RefWrapperType, IEntityTables>();

        public IEntityTable<TR> FindTable<TR>(in ExclusiveGroupStruct groupID)
            where TR : class, IEntityRow
        {
            if (!_groupToTable.TryGetValue(groupID, out var table))
            {
                foreach (var schemaMetadata in registeredSchemas)
                {
                    if (schemaMetadata.groupToTable.TryGetValue(groupID, out var tableNode))
                    {
                        table = tableNode.table;
                        break;
                    }
                }

                _groupToTable[groupID] = table;
            }

            // return null if type does not match
            return table as IEntityTable<TR>;
        }

        /// <summary>
        /// This will find all Tables containing Row type TR
        /// </summary>
        public IEntityTables<TR> FindTables<TR>()
            where TR : class, IEntityRow
        {
            if (!_rowToTables.TryGetValue(TypeRefWrapper<TR>.wrapper, out var tables))
            {
                var tablesList = new FasterList<IEntityTable<TR>>();

                foreach (var schemaMetadata in registeredSchemas)
                {
                    var tableNodes = schemaMetadata.groupToTable.GetValues(out var count);

                    for (int i = 0; i < count; ++i)
                    {
                        if (tableNodes[i].table is IEntityTable<TR> matchingTable)
                            tablesList.Add(matchingTable);
                    }
                }

                tables = new CombinedTables<TR>(tablesList);
                _rowToTables[TypeRefWrapper<TR>.wrapper] = tables;
            }

            return (IEntityTables<TR>)tables;
        }
    }
}
