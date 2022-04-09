using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct JoinQueryEnumerator<TResult, TJoined, TJoinComponent>
        where TResult : struct, IResultSet
        where TJoined : struct, IResultSet
        where TJoinComponent : unmanaged, IForeignKeyComponent
    {
        internal QueryEnumerator<TResult, TJoinComponent> _inner;
        private readonly IJoinProvider _joiner;
        internal readonly uint _joinedFilterIndex;

        private readonly FasterDictionary<ExclusiveGroupStruct, int> _joinedGroups;
        private NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private int _joinedGroupIndex;

        internal TJoined _joinedResult;
        internal ExclusiveGroupStruct _joinedGroup;
        internal NativeEntityIDs _entityIDs;

        internal JoinQueryEnumerator(ResultSetQueryConfig config, IJoinProvider joiner) : this()
        {
            _inner = new(config);
            _joiner = joiner;
            _joinedFilterIndex = (uint)config.filters.count;

            if (indexedDB.indexers.TryGetValue(joiner.IndexerID.id, out var indexerData) &&
                indexerData is IndexerData<ExclusiveGroupStruct> indexerDataGroup)
            {
                _joinedGroups = indexerDataGroup.keyToFilterID;
            }

            _joinedGroupIndex = -1;
        }

        public bool MoveNext()
        {
            if (_joinedGroups == null)
                return false;

            while (true)
            {
                if (_inner._groupIndex < 0)
                    FindNextGroup();

                if (_joinedGroupIndex >= _joinedGroups.count)
                    return false;

                if (_inner.MoveNext())
                    return true;

                _inner.Reset();
            }
        }

        private void FindNextGroup()
        {
            while (++_joinedGroupIndex < _joinedGroups.count)
            {
                _joinedGroup = _joinedGroups.unsafeKeys[_joinedGroupIndex].key;

                if (_joiner.IsValidGroup(indexedDB, _joinedGroup))
                {
                    var filterID = new CombinedFilterID(
                        _joinedGroups.unsafeValues[_joinedGroupIndex], _joiner.IndexerID);

                    ref var filter = ref indexedDB.GetOrAddPersistentFilter(filterID);

                    if (filter.groupCount == 0)
                        continue;

                    _inner._config.filters.AddAt(_joinedFilterIndex, filter);

                    _egidMapper = indexedDB.GetNativeEGIDMapper(_joinedGroup);
                    _entityIDs = indexedDB.QueryEntityIDs(_joinedGroup);

                    ResultSetHelper<TJoined>.Assign(out _joinedResult, indexedDB.entitiesDB, _joinedGroup);
                    break;
                }
            }
        }

        public void Reset()
        {
            _inner.Reset();
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        private IndexedDB indexedDB => _inner._config.indexedDB;

        internal JoinedIndexedIndices<TJoinComponent> Indices
            => new(_inner.Indices, indexedDB.entitiesDB.GetEntityLocator(), _egidMapper, _inner._components);

        public QueryResult<TResult, TJoined, TJoinComponent> Current
            => new(_inner._result, _inner._groups[_inner._groupIndex], _joinedResult, _joinedGroup, Indices, _entityIDs);
    }
}
