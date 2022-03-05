using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // TODO: if apply new filter system, we can remove handling for 'MovedTo' and 'Remove'
    // but we still need 'Add' to make sure new entities are included in index
    internal class TableIndexingEngine<TR, TK, TC> : ReactToRowEngine<TR, TC>
        where TR : class, IIndexableRow<TC>
        where TK : unmanaged
        where TC : unmanaged, IIndexedComponent
    {
        private readonly HashSet<IndexerGroupData> _groupsToRebuild = new HashSet<IndexerGroupData>();

        private readonly Func<TC, TK> _keyGetter;

        public TableIndexingEngine(IndexedDB indexedDB, Func<TC, TK> keyGetter) : base(indexedDB)
        {
            _keyGetter = keyGetter;
        }

        public override void Add(ref TC component, IEntityTable<TR> table, uint entityID)
        {
            CheckAdd(ref component, table);
        }

        public override void MovedTo(ref TC component, IEntityTable<TR> previousTable, IEntityTable<TR> table, uint entityID)
        {
            CheckRemove(ref component, previousTable);
            CheckAdd(ref component, table);
        }

        public override void Remove(ref TC component, IEntityTable<TR> table, uint entityID)
        {
            CheckRemove(ref component, table);
        }

        private void CheckAdd(ref TC keyComponent, IEntityTable<TR> table)
        {
            var indexers = indexedDB.FindIndexers<TK, TC>(table.ExclusiveGroup);

            foreach (var indexer in indexers)
            {
                AddToFilter(indexer.IndexerID, ref keyComponent, table);
            }
        }

        private void CheckRemove(ref TC keyComponent, IEntityTable<TR> table)
        {
            var indexers = indexedDB.FindIndexers<TK, TC>(table.ExclusiveGroup);

            foreach (var indexer in indexers)
            {
                RemoveFromFilter(indexer.IndexerID, ref keyComponent, table);
            }
        }

        private void AddToFilter(int indexerID, ref TC keyComponent, IEntityTable<TR> table)
        {
            ref var groupData = ref indexedDB.CreateOrGetIndexedGroupData(
                indexerID, _keyGetter(keyComponent), table);

            var mapper = indexedDB.entitiesDB.QueryMappedEntities<RowIdentityComponent>(table.ExclusiveGroup);

            groupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private void RemoveFromFilter(int indexerID, ref TC keyComponent, IEntityTable<TR> table)
        {
            ref var groupData = ref indexedDB.CreateOrGetIndexedGroupData(
                indexerID, _keyGetter(keyComponent), table);

            groupData.filter.TryRemove(keyComponent.ID.entityID);

            // filter will be rebuilt when submission is done
            _groupsToRebuild.Add(groupData);
        }

        public void EntitiesSubmitted()
        {
            foreach (var groupData in _groupsToRebuild)
            {
                var mapper = indexedDB.entitiesDB.QueryMappedEntities<TC>(groupData.table.ExclusiveGroup);
                groupData.filter.RebuildIndicesOnStructuralChange(mapper);
            }

            _groupsToRebuild.Clear();
        }
    }
}
