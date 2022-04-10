using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        internal readonly FasterDictionary<ExclusiveGroupStruct, ITableDefinition> _groupToTable
            = new FasterDictionary<ExclusiveGroupStruct, ITableDefinition>();

        internal readonly FasterDictionary<RefWrapperType, ITablesDefinition> _rowToTables
            = new FasterDictionary<RefWrapperType, ITablesDefinition>();

        public ITableDefinition FindTable(in ExclusiveGroupStruct groupID)
        {
            return _groupToTable[groupID];
        }

        public IEntityTable<TR> FindTable<TR>(in ExclusiveGroupStruct groupID)
            where TR : class, IEntityRow
        {
            // return null if type does not match
            return FindTable(groupID) as IEntityTable<TR>;
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
                    foreach (var table in schemaMetadata.tables)
                    {
                        if (table is IEntityTable<TR> matchingTable)
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
