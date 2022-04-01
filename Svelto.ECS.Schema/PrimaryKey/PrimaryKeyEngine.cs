using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class PrimaryKeyEngine :
        ReactToRowEngine<IPrimaryKeyRow, RowIdentityComponent>, IStepEngine, IReactOnSubmission
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
            var keyData = indexedDB.entitiesToUpdateGroup.GetIndexerKeyData(indexedDB);

            for (int filterIndex = 0; filterIndex < keyData.groups.count; ++filterIndex)
            {
                var group = keyData.groups.unsafeKeys[filterIndex].key;
                var groupData = keyData.groups.unsafeValues[filterIndex];

                var table = indexedDB.FindTable(group);

                if (table.PrimaryKeys.count == 0)
                    continue;

                var pks = table.PrimaryKeys.GetValues(out var pkCount);

                for (int p = 0; p < pkCount; ++p)
                {
                    pks[p].Ready(indexedDB.entitiesDB, group);
                }

                var indices = new IndexedIndices(groupData.filter.filteredIndices);

                var (egid, _) = indexedDB.entitiesDB.QueryEntities<EGIDComponent>(group);

                foreach (var i in indices)
                {
                    int groupIndex = 0;

                    for (int p = 0; p < pkCount; ++p)
                    {
                        groupIndex *= pks[p].PossibleKeyCount;
                        groupIndex += pks[p].QueryGroupIndex(i);
                    }

                    ExclusiveBuildGroup targetGroup = table.Group + (uint)(groupIndex + 1);

                    table.Swap(indexedDB.entityFunctions, egid[i].ID, targetGroup);
                }
            }

            indexedDB.Memo(indexedDB.entitiesToUpdateGroup).Clear();
        }

        public void EntitiesSubmitted()
        {
            Step();
        }
    }
}