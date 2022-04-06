using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct SelectFromJoinQueryEnumerator<TResult, TJoined, TJoinComponent>
        where TResult : struct, IResultSet
        where TJoined : struct, IResultSet
        where TJoinComponent : unmanaged, IForeignKeyComponent
    {
        private SelectFromQueryEnumerator<TResult, TJoinComponent> _inner;
        private readonly IJoinProvider _joiner;
        private readonly uint _joinedFilterIndex;

        private readonly FasterDictionary<ExclusiveGroupStruct, int> _joinedGroups;
        private ExclusiveGroupStruct _joinedGroup;
        private NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private int _joinedGroupIndex;

        private TJoined _joinedResult;

        internal SelectFromJoinQueryEnumerator(ResultSetQueryConfig config, IJoinProvider joiner) : this()
        {
            _inner = new(config);
            _joiner = joiner;
            _joinedFilterIndex = (uint)config.filters.count;

            if (config.indexedDB.indexers.TryGetValue(joiner.IndexerID.id, out var indexerData) &&
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

                if (_joiner.IsValidGroup(_inner._config.indexedDB, _joinedGroup))
                {
                    var filterID = new CombinedFilterID(
                        _joinedGroups.unsafeValues[_joinedGroupIndex], _joiner.IndexerID);

                    ref var filter = ref _inner._config.indexedDB.GetOrAddPersistentFilter(filterID);

                    if (filter.groupCount == 0)
                        continue;

                    _inner._config.filters.AddAt(_joinedFilterIndex, filter);

                    _egidMapper = _inner._config.indexedDB.GetNativeEGIDMapper(_joinedGroup);

                    ResultSetHelper<TJoined>.Assign(out _joinedResult, _inner._config.indexedDB.entitiesDB, _joinedGroup);
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

        public SelectFromJoinQueryResult<TResult, TJoined, TJoinComponent> Current =>
            new(_inner._result, _inner._groups[_inner._groupIndex], _joinedResult, _joinedGroup,
            new(_inner._config.temporaryEntityIndices, _inner._config.temporaryFilters,
                _inner._config.indexedDB.entitiesDB.GetEntityLocator(), _egidMapper,
                _inner._components, _inner._entityIDs, _inner._count));
    }
}
