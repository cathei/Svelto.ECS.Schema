using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct JoinQueryEnumerator<TResult, TJoined1, TJoined2, TJoinComponent1, TJoinComponent2>
        where TResult : struct, IResultSet
        where TJoined1 : struct, IResultSet
        where TJoined2 : struct, IResultSet
        where TJoinComponent1 : unmanaged, IForeignKeyComponent
        where TJoinComponent2 : unmanaged, IForeignKeyComponent
    {
        private JoinQueryEnumerator<TResult, TJoined1, TJoinComponent1> _inner;
        private readonly IJoinProvider _joiner;
        private readonly uint _joinedFilterIndex;

        private readonly FasterDictionary<ExclusiveGroupStruct, int> _joinedGroups;
        private ExclusiveGroupStruct _joinedGroup;
        private NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private int _joinedGroupIndex;

        private TJoined2 _joinedResult;
        private NB<TJoinComponent2> _components;
        private NativeEntityIDs _entityIDs;

        internal JoinQueryEnumerator(ResultSetQueryConfig config, IJoinProvider joiner1, IJoinProvider joiner2) : this()
        {
            _inner = new(config, joiner1);
            _joiner = joiner2;
            _joinedFilterIndex = _inner._joinedFilterIndex + 1;

            if (config.indexedDB.indexers.TryGetValue(_joiner.IndexerID.id, out var indexerData) &&
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
                if (_inner._inner._groupIndex < 0)
                    FindNextGroup();

                if (_joinedGroupIndex >= _joinedGroups.count)
                    return false;

                if (_inner.MoveNext())
                {
                    (_components, _) = indexedDB.entitiesDB.QueryEntities<TJoinComponent2>(
                        _inner._inner._groups[_inner._inner._groupIndex]);

                    return true;
                }
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

                    _inner._inner._config.filters.AddAt(_joinedFilterIndex, filter);

                    _egidMapper = indexedDB.GetNativeEGIDMapper(_joinedGroup);
                    _entityIDs = indexedDB.QueryEntityIDs(_joinedGroup);

                    ResultSetHelper<TJoined2>.Assign(out _joinedResult, indexedDB.entitiesDB, _joinedGroup);
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

        private IndexedDB indexedDB => _inner._inner._config.indexedDB;

        internal JoinedIndexedIndices<TJoinComponent1, TJoinComponent2> Indices
            => new(_inner.Indices, _egidMapper, _components);

        public QueryResult<TResult, TJoined1, TJoined2, TJoinComponent1, TJoinComponent2> Current
            => new(_inner._inner._result, _inner._inner._groups[_inner._inner._groupIndex],
            _inner._joinedResult, _inner._joinedGroup, _joinedResult, _joinedGroup,
            Indices, _inner._entityIDs, _entityIDs);
    }
}
