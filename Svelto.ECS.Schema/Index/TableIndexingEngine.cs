using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{
    internal class TableIndexingEngine<T> : IReactOnAddAndRemove<Indexed<T>>, IReactOnSwap<Indexed<T>>, IReactOnSubmission
        where T : unmanaged, IEntityIndexKey<T>
    {
        private readonly IndexesDB _indexesDB;

        private readonly HashSet<IndexesDB.IndexerGroupData> _groupsToRebuild = new HashSet<IndexesDB.IndexerGroupData>();

        private static RefWrapperType IndexKeyType => TypeRefWrapper<T>.wrapper;

        public TableIndexingEngine(IndexesDB indexesDB)
        {
            _indexesDB = indexesDB;
        }

        public void Add(ref Indexed<T> keyComponent, EGID egid)
        {
            CheckAdd(ref keyComponent, egid.groupID);
        }

        public void MovedTo(ref Indexed<T> keyComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            CheckRemove(ref keyComponent, previousGroup);
            CheckAdd(ref keyComponent, egid.groupID);
        }

        public void Remove(ref Indexed<T> keyComponent, EGID egid)
        {
            CheckRemove(ref keyComponent, egid.groupID);
        }

        private void CheckAdd(ref Indexed<T> keyComponent, in ExclusiveGroupStruct groupId)
        {
            if (_indexesDB.TryGetShard(groupId, out var node))
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
                                AddToFilter(indexer.IndexerId, ref keyComponent, groupId);
                            }
                        }
                    }

                    node = node.parent;
                }
            }
        }

        private void AddToFilter(int indexerId, ref Indexed<T> keyComponent, in ExclusiveGroupStruct groupId)
        {
            ref var groupData = ref _indexesDB.CreateOrGetGroupData(indexerId, keyComponent.Key, groupId);

            var mapper = _indexesDB.entitiesDB.QueryMappedEntities<Indexed<T>>(groupId);

            groupData.filter.Add(keyComponent.ID.entityID, mapper);
        }

        private void CheckRemove(ref Indexed<T> keyComponent, in ExclusiveGroupStruct groupId)
        {
            if (_indexesDB.TryGetShard(groupId, out var node))
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
                                RemoveFromFilter(indexer.IndexerId, ref keyComponent, groupId);
                            }
                        }
                    }

                    node = node.parent;
                }
            }
        }

        private void RemoveFromFilter(int indexerId, ref Indexed<T> keyComponent, in ExclusiveGroupStruct groupId)
        {
            ref var groupData = ref _indexesDB.CreateOrGetGroupData<T>(indexerId, keyComponent.Key, groupId);

            groupData.filter.TryRemove(keyComponent.ID.entityID);

            // filter will be rebuilt when submission is done
            _groupsToRebuild.Add(groupData);
        }

        public void EntitiesSubmitted()
        {
            foreach (var groupData in _groupsToRebuild)
            {
                var mapper = _indexesDB.entitiesDB.QueryMappedEntities<Indexed<T>>(groupData.group);
                groupData.filter.RebuildIndicesOnStructuralChange(mapper);
            }

            _groupsToRebuild.Clear();
        }
    }
}
