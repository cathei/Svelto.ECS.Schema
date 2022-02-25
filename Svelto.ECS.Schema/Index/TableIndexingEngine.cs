using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    // TODO: if apply new filter system, we can remove 'CheckRemove'
    // but we still need 'CheckAdd' to make sure indexes are up-to-date
    internal class TableIndexingEngine<T> :
            IReactOnAddAndRemove<IEntityIndexKey<T>.Component>,
            IReactOnSwap<IEntityIndexKey<T>.Component>,
            IReactOnSubmission
        where T : unmanaged, IEntityIndexKey<T>
    {
        private readonly IndexesDB _indexesDB;

        private readonly HashSet<IndexesDB.IndexerGroupData> _groupsToRebuild = new HashSet<IndexesDB.IndexerGroupData>();

        private static RefWrapperType IndexKeyType => TypeRefWrapper<T>.wrapper;

        public TableIndexingEngine(IndexesDB indexesDB)
        {
            _indexesDB = indexesDB;
        }

        public void Add(ref IEntityIndexKey<T>.Component keyComponent, EGID egid)
        {
            CheckAdd(ref keyComponent, egid.groupID);
        }

        public void MovedTo(ref IEntityIndexKey<T>.Component keyComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            CheckRemove(ref keyComponent, previousGroup);
            CheckAdd(ref keyComponent, egid.groupID);
        }

        public void Remove(ref IEntityIndexKey<T>.Component keyComponent, EGID egid)
        {
            CheckRemove(ref keyComponent, egid.groupID);
        }

        private void CheckAdd(ref IEntityIndexKey<T>.Component keyComponent, in ExclusiveGroupStruct groupID)
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
        }

        private void AddToFilter(int indexerID, ref IEntityIndexKey<T>.Component keyComponent, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref _indexesDB.CreateOrGetIndexerGroup(indexerID, keyComponent.Key, groupID);

            var mapper = _indexesDB.entitiesDB.QueryMappedEntities<IEntityIndexKey<T>.Component>(groupID);

            groupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private void CheckRemove(ref IEntityIndexKey<T>.Component keyComponent, in ExclusiveGroupStruct groupID)
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
        }

        private void RemoveFromFilter(int indexerID, ref IEntityIndexKey<T>.Component keyComponent, in ExclusiveGroupStruct groupID)
        {
            ref var groupData = ref _indexesDB.CreateOrGetIndexerGroup(indexerID, keyComponent.Key, groupID);

            groupData.filter.TryRemove(keyComponent.ID.entityID);

            // filter will be rebuilt when submission is done
            _groupsToRebuild.Add(groupData);
        }

        public void EntitiesSubmitted()
        {
            foreach (var groupData in _groupsToRebuild)
            {
                var mapper = _indexesDB.entitiesDB.QueryMappedEntities<IEntityIndexKey<T>.Component>(groupData.group);
                groupData.filter.RebuildIndicesOnStructuralChange(mapper);
            }

            _groupsToRebuild.Clear();
        }
    }
}
