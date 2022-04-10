using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;
using Svelto.ObjectPool;

namespace Svelto.ECS.Schema.Internal
{
    public readonly ref struct SelectFromQuery<TRow, TResult>
        where TRow : class, IEntityRow
        where TResult : struct, IResultSet
    {
        internal readonly ResultSetQueryConfig config;
        internal readonly IEntityTables<TRow> tables;

        internal SelectFromQuery(IndexedDB indexedDB)
        {
            config = ResultSetQueryConfig.Use();
            config.indexedDB = indexedDB;
            this.tables = indexedDB.FindTables<TRow>();
        }

        internal SelectFromQuery(IndexedDB indexedDB, IEntityTables<TRow> tables)
        {
            config = ResultSetQueryConfig.Use();
            config.indexedDB = indexedDB;
            this.tables = tables;
        }

        public SelectFromQuery<TRow, TResult> Where<T>(T query)
            where T : IWhereQuery<TRow>
        {
            if (config.temporaryGroups.count != 0)
                throw new ECSException("Query is already in use, cannot add Where condition");

            query.Apply(config);
            return this;
        }

        public SelectFromJoinQuery<TRow, TResult, TJoined> Join<TJoined>()
            where TJoined : struct, IResultSet 
        {
            if (config.temporaryGroups.count != 0)
                throw new ECSException("Query is already in use, cannot perform Join");

            Build();
            return new(config);
        }

        public QueryEnumerator<TResult, RowIdentityComponent> GetEnumerator()
        {
            Build();
            return new(config);
        }

        internal void Build()
        {
            if (config.temporaryGroups.count != 0)
                return;

            int tableCount = tables.Range;

            for (int i = 0; i < tableCount; ++i)
            {
                var table = tables.GetTable(i);

                bool hasAllKeys = true;

                foreach (var pkID in config.pkToValue.keys)
                {
                    if (!table.PrimaryKeys.ContainsKey(pkID))
                    {
                        hasAllKeys = false;
                        break;
                    }
                }

                if (!hasAllKeys)
                    continue;

                if (table.PrimaryKeys.count == 0)
                    config.temporaryGroups.Add(table.Group, table.Group);
                else
                    IterateGroup(table);
            }
        }

        private void IterateGroup(IEntityTable<TRow> table, int groupIndex = 0, int depth = 0)
        {
            if (depth >= table.PrimaryKeys.count)
            {
                // table group index 0 is reserved for adding only
                var group = table.Group + (uint)(groupIndex + 1);
                config.temporaryGroups.Add(group, group);
                return;
            }

            var pk = table.PrimaryKeys.unsafeValues[depth];

            // mutiply parent index
            groupIndex *= pk.PossibleKeyCount;

            // when sub-index applied
            if (config.pkToValue.TryGetValue(pk.PrimaryKeyID, out var value))
            {
                groupIndex += value;
                IterateGroup(table, groupIndex, depth + 1);
                return;
            }

            // iterate all subgroup
            for (int i = 0; i < pk.PossibleKeyCount; ++i)
            {
                IterateGroup(table, groupIndex + i, depth + 1);
            }
        }
    }
}
