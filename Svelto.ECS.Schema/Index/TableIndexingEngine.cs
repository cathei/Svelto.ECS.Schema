using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    // TODO: if apply new filter system, we can remove handling for 'MovedTo' and 'Remove'
    // but we still need 'Add' to make sure new entities are included in index
    internal class TableIndexingEngine<TK, TC> :
            IReactOnAddAndRemove<TC>, IReactOnSwap<TC>, IReactOnSubmission
        where TK : unmanaged
        where TC : unmanaged, IIndexedComponent<TK>
    {
        private readonly IndexedDB _indexedDB;

        private readonly HashSet<IndexedGroupData> _groupsToRebuild = new HashSet<IndexedGroupData>();

        public TableIndexingEngine(IndexedDB indexedDB)
        {
            _indexedDB = indexedDB;
        }

        public void Add(ref TC keyComponent, EGID egid)
        {
            CheckAdd(ref keyComponent, egid.groupID);
        }

        public void MovedTo(ref TC keyComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            CheckRemove(ref keyComponent, previousGroup);
            CheckAdd(ref keyComponent, egid.groupID);
        }

        public void Remove(ref TC keyComponent, EGID egid)
        {
            CheckRemove(ref keyComponent, egid.groupID);
        }

        private void CheckAdd(ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            var indexers = _indexedDB.FindIndexers<TK, TC>(groupID);

            foreach (var indexer in indexers)
            {
                AddToFilter(indexer.IndexerID, ref keyComponent, groupID);
            }
        }

        private void CheckRemove(ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            var indexers = _indexedDB.FindIndexers<TK, TC>(groupID);

            foreach (var indexer in indexers)
            {
                RemoveFromFilter(indexer.IndexerID, ref keyComponent, groupID);
            }
        }

        private void AddToFilter(int indexerID, ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref _indexedDB.CreateOrGetIndexedGroupData<TK, TC>(
                indexerID, keyComponent.Value, groupID);

            var mapper = _indexedDB.entitiesDB.QueryMappedEntities<TC>(groupID);

            groupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private void RemoveFromFilter(int indexerID, ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref _indexedDB.CreateOrGetIndexedGroupData<TK, TC>(
                indexerID, keyComponent.Value, groupID);

            groupData.filter.TryRemove(keyComponent.ID.entityID);

            // filter will be rebuilt when submission is done
            _groupsToRebuild.Add(groupData);
        }

        public void EntitiesSubmitted()
        {
            foreach (var groupData in _groupsToRebuild)
            {
                var mapper = _indexedDB.entitiesDB.QueryMappedEntities<TC>(groupData.group);
                groupData.filter.RebuildIndicesOnStructuralChange(mapper);
            }

            _groupsToRebuild.Clear();
        }
    }
}
