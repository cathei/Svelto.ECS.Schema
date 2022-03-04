using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    // TODO: if apply new filter system, we can remove handling for 'MovedTo' and 'Remove'
    // but we still need 'Add' to make sure new entities are included in index
    internal class TableIndexingEngine<TR, TK, TC> : IReactiveRow<TR, TC>.Engine
        where TR : class, IIndexableRow<TK, TC>, IReactiveRow<TR, TC>
        where TK : unmanaged
        where TC : unmanaged, IIndexableComponent<TK>
    {
        private readonly HashSet<IndexedGroupData<TR>> _groupsToRebuild = new HashSet<IndexedGroupData<TR>>();

        public TableIndexingEngine(IndexedDB indexedDB) : base(indexedDB) { }

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
            ref var groupData = ref indexedDB.CreateOrGetIndexedGroupData<TR, TK, TC>(
                indexerID, keyComponent.Value, table);

            var mapper = indexedDB.entitiesDB.QueryMappedEntities<TC>(table.ExclusiveGroup);

            groupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private void RemoveFromFilter(int indexerID, ref TC keyComponent, IEntityTable<TR> table)
        {
            ref var groupData = ref indexedDB.CreateOrGetIndexedGroupData<TR, TK, TC>(
                indexerID, keyComponent.Value, table);

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
