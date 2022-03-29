using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class PrimaryKeyEngine :
        ReactToRowEngine<IPrimaryKeyRow, RowIdentityComponent>, IStepEngine
    {
        public PrimaryKeyEngine(IndexedDB indexedDB) : base(indexedDB) { }

        public string name { get; } = nameof(PrimaryKeyEngine);

        protected override void Add(ref RowIdentityComponent entityComponent, IEntityTable<IPrimaryKeyRow> table, EGID egid)
        {
            // process only build group (no. 0)
            if (egid.groupID != table.Group)
                return;

            indexedDB.Memo(indexedDB.entitiesToUpdateGroup).Add(egid.entityID, egid.groupID);
        }

        protected override void MovedTo(ref RowIdentityComponent entityComponent, IEntityTable<IPrimaryKeyRow> previousTable, IEntityTable<IPrimaryKeyRow> table, EGID egid)
        {
            // process only build group (no. 0)
            if (egid.groupID != table.Group)
                return;

            indexedDB.Memo(indexedDB.entitiesToUpdateGroup).Add(egid.entityID, egid.groupID);
        }

        public void Step()
        {
            using var query = indexedDB.From<IPrimaryKeyRow>().Where(indexedDB.entitiesToUpdateGroup);

            foreach (var result in query.Select<IndexableResultSet<EGIDComponent>>())
            {
                var table = indexedDB.FindTable(result.group);

                if (table.PrimaryKeys.count == 0)
                    continue;

                foreach (var pk in table.PrimaryKeys)
                {
                    pk.Ready(indexedDB.entitiesDB, result.group);
                }

                foreach (var i in result.indices)
                {
                    int groupIndex = 0;

                    foreach (var pk in table.PrimaryKeys)
                    {
                        groupIndex *= pk.PossibleKeyCount;
                        groupIndex += pk.QueryGroupIndex(i);
                    }

                    ExclusiveBuildGroup targetGroup = table.Group + (uint)(groupIndex + 1);

                    table.Swap(indexedDB.entityFunctions, result.set.egid[i].ID, targetGroup);
                }
            }

            indexedDB.Memo(indexedDB.entitiesToUpdateGroup).Clear();
        }
    }
}