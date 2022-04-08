using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal class PrimaryKeyEngine :
        IReactRowAdd<IPrimaryKeyRow, PKIdentityComponent>,
        IReactRowSwap<IPrimaryKeyRow, PKIdentityComponent>,
        IStepEngine
    {
        public PrimaryKeyEngine(IndexedDB indexedDB)
        {
            this.indexedDB = indexedDB;
        }

        public IndexedDB indexedDB { get; }

        public string name { get; } = nameof(PrimaryKeyEngine);

        public void Add(in EntityCollection<PKIdentityComponent> collection,
            RangedIndices indices, ExclusiveGroupStruct group)
        {
            // process only build group (no. 0)
            var table = indexedDB.FindTable(group);

            if (group != table.Group)
                return;

            var (_, entityIDs, _) = collection;

            Process(entityIDs, indices, group);
        }

        public void MovedTo(in EntityCollection<PKIdentityComponent> collection,
            RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            // process only build group (no. 0)
            var table = indexedDB.FindTable(toGroup);

            if (toGroup != table.Group)
                return;

            var (_, entityIDs, _) = collection;

            Process(entityIDs, indices, toGroup);
        }

        public void Process(NativeEntityIDs entityIDs, in RangedIndices indices, ExclusiveGroupStruct group)
        {
            var table = indexedDB.FindTable(group);

            if (table.PrimaryKeys.count == 0)
                return;

            var pks = table.PrimaryKeys.GetValues(out var pkCount);

            for (int p = 0; p < pkCount; ++p)
            {
                pks[p].Ready(indexedDB.entitiesDB, group);
            }

            foreach (var i in indices)
            {
                int groupIndex = 0;

                for (int p = 0; p < pkCount; ++p)
                {
                    groupIndex *= pks[p].PossibleKeyCount;
                    groupIndex += pks[p].QueryGroupIndex(i);
                }

                ExclusiveGroupStruct targetGroup = table.Group + (uint)(groupIndex + 1);

                if (group.id != targetGroup.id)
                    table.Swap(indexedDB.entityFunctions, new EGID(entityIDs[i], group), targetGroup);
            }
        }

        public void Step()
        {
            foreach (var result in indexedDB.FromAll<IPrimaryKeyRow>().Where(indexedDB.entitiesToUpdateGroup))
            {
                var group = result.group;
                var table = indexedDB.FindTable(group);

                if (table.PrimaryKeys.count == 0)
                    return;

                var pks = table.PrimaryKeys.GetValues(out var pkCount);

                for (int p = 0; p < pkCount; ++p)
                {
                    pks[p].Ready(indexedDB.entitiesDB, group);
                }

                foreach (var i in result.indices)
                {
                    int groupIndex = 0;

                    for (int p = 0; p < pkCount; ++p)
                    {
                        groupIndex *= pks[p].PossibleKeyCount;
                        groupIndex += pks[p].QueryGroupIndex(i);
                    }

                    ExclusiveGroupStruct targetGroup = table.Group + (uint)(groupIndex + 1);

                    if (group.id != targetGroup.id)
                        table.Swap(indexedDB.entityFunctions, result.egid[i], targetGroup);
                }
            }

            indexedDB.Memo(indexedDB.entitiesToUpdateGroup).Clear();
        }
    }
}