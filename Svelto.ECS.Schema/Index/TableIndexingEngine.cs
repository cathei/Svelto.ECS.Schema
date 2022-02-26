using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // TODO: if apply new filter system, we can remove 'CheckRemove'
    // but we still need 'CheckAdd' to make sure indexes are up-to-date
    internal class TableIndexingEngine<TK, TC> :
            IReactOnAddAndRemove<TC>,
            IReactOnSwap<TC>,
            IReactOnSubmission
        where TK : unmanaged, IKeyEquatable<TK>
        where TC : unmanaged, IIndexedComponent<TK>
    {
        private readonly IndexesDB _indexesDB;

        private readonly HashSet<IndexesDB.IndexerGroupData> _groupsToRebuild = new HashSet<IndexesDB.IndexerGroupData>();

        private static RefWrapperType IndexKeyType => TypeRefWrapper<TK>.wrapper;

        public TableIndexingEngine(IndexesDB indexesDB)
        {
            _indexesDB = indexesDB;
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
            if (_indexesDB.TryGetShard(groupID, out var node))
            {
                while (node != null)
                {
                    if (node.indexers != null)
                    {
                        for (int i = 0; i < node.indexers.count; ++i)
                        {
                            var indexer = node.indexers[i];

                            if (indexer.KeyType.Equals(IndexKeyType))
                            {
                                AddToFilter(indexer.IndexerID, ref keyComponent, groupID);
                            }
                        }
                    }

                    node = node.parent;
                }
            }

            for (int i = 0; i < _indexesDB.stateMachineIndexers.count; ++i)
            {
                var indexer = _indexesDB.stateMachineIndexers[i];

                if (indexer.KeyType.Equals(IndexKeyType))
                {
                    AddToFilter(indexer.IndexerID, ref keyComponent, groupID);
                }
            }
        }

        private void AddToFilter(int indexerID, ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref _indexesDB.CreateOrGetIndexerGroup<TK, TC>(
                indexerID, keyComponent.Key, groupID);

            var mapper = _indexesDB.entitiesDB.QueryMappedEntities<TC>(groupID);

            groupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private void CheckRemove(ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            if (_indexesDB.TryGetShard(groupID, out var node))
            {
                while (node != null)
                {
                    if (node.indexers != null)
                    {
                        for (int i = 0; i < node.indexers.count; ++i)
                        {
                            var indexer = node.indexers[i];

                            if (indexer.KeyType.Equals(IndexKeyType))
                            {
                                RemoveFromFilter(indexer.IndexerID, ref keyComponent, groupID);
                            }
                        }
                    }

                    node = node.parent;
                }
            }

            for (int i = 0; i < _indexesDB.stateMachineIndexers.count; ++i)
            {
                var indexer = _indexesDB.stateMachineIndexers[i];

                if (indexer.KeyType.Equals(IndexKeyType))
                {
                    RemoveFromFilter(indexer.IndexerID, ref keyComponent, groupID);
                }
            }
        }

        private void RemoveFromFilter(int indexerID, ref TC keyComponent, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref _indexesDB.CreateOrGetIndexerGroup<TK, TC>(
                indexerID, keyComponent.Key, groupID);

            groupData.filter.TryRemove(keyComponent.ID.entityID);

            // filter will be rebuilt when submission is done
            _groupsToRebuild.Add(groupData);
        }

        public void EntitiesSubmitted()
        {
            foreach (var groupData in _groupsToRebuild)
            {
                var mapper = _indexesDB.entitiesDB.QueryMappedEntities<TC>(groupData.group);
                groupData.filter.RebuildIndicesOnStructuralChange(mapper);
            }

            _groupsToRebuild.Clear();
        }
    }
}
