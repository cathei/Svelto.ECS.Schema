using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public sealed partial class IndexedDB
    {
        internal readonly FasterDictionary<ExclusiveGroupStruct, ITableDefinition> _groupToTable = new();
        internal readonly FasterDictionary<RefWrapperType, ITablesDefinition> _rowToTables = new();

        public ITableDefinition FindTable(in ExclusiveGroupStruct groupID)
        {
            return _groupToTable[groupID];
        }

        public IEntityTable<TRow> FindTable<TRow>(in ExclusiveGroupStruct groupID)
            where TRow : class, IEntityRow
        {
            // return null if type does not match
            return FindTable(groupID) as IEntityTable<TRow>;
        }

        /// <summary>
        /// This will find all Tables containing Row type TR
        /// </summary>
        public IEntityTables<TRow> FindTables<TRow>()
            where TRow : class, IEntityRow
        {
            if (!_rowToTables.TryGetValue(TypeRefWrapper<TRow>.wrapper, out var tables))
            {
                var tablesList = new FasterList<IEntityTable<TRow>>();

                foreach (var schemaMetadata in registeredSchemas)
                {
                    foreach (var table in schemaMetadata.tables)
                    {
                        if (table is IEntityTable<TRow> matchingTable)
                            tablesList.Add(matchingTable);
                    }
                }

                tables = new CombinedTables<TRow>(tablesList);
                _rowToTables[TypeRefWrapper<TRow>.wrapper] = tables;
            }

            return (IEntityTables<TRow>)tables;
        }
    }
}
