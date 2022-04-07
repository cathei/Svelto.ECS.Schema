using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct QueryEnumerator<TResult, TComponent>
        where TResult : struct, IResultSet
        where TComponent : unmanaged, IEntityComponent
    {
        internal readonly ResultSetQueryConfig _config;
        internal readonly NB<ExclusiveGroupStruct> _groups;
        private readonly uint _groupCount;

        internal int _groupIndex;
        internal TResult _result;

        internal NB<TComponent> _components;
        internal NativeEntityIDs _entityIDs;
        internal int _count;

        internal QueryEnumerator(ResultSetQueryConfig config) : this()
        {
            _config = config;

            _groups = _config.temporaryGroups.GetValues(out _groupCount);
            _groupIndex = -1;
        }

        public bool MoveNext()
        {
            while (++_groupIndex < _groupCount)
            {
                var currentGroup = _groups[_groupIndex];

                bool haveAllFilters = true;

                for (int i = 0; i < _config.filters.count; ++i)
                {
                    var filter = _config.filters[i];
                    var groupFilter = filter.GetGroupFilter(currentGroup);

                    if (groupFilter.count == 0)
                    {
                        haveAllFilters = false;
                        break;
                    }

                    _config.temporaryFilters.AddAt((uint)i) = groupFilter;
                }

                if (haveAllFilters)
                {
                    (_components, _entityIDs, _count) =
                        _config.indexedDB.entitiesDB.QueryEntities<TComponent>(currentGroup);

                    if (_count == 0)
                        continue;

                    if (_config.selectedEntityIDs.Count > 0)
                    {
                        var egidMapper = _config.indexedDB.GetEGIDMapper(currentGroup);

                        _config.temporaryEntityIndices.Clear();

                        foreach (uint entityID in _config.selectedEntityIDs)
                        {
                            if (egidMapper.FindIndex(entityID, out var index))
                                _config.temporaryEntityIndices.Add(index);
                        }

                        if (_config.temporaryEntityIndices.count == 0)
                            continue;
                    }

                    ResultSetHelper<TResult>.Assign(out _result, _config.indexedDB.entitiesDB, currentGroup);
                    return true;
                }
            }

            return false;
        }

        public void Reset() { _groupIndex = -1; }

        public void Dispose()
        {
            // TODO: auto-close option might be needed
            ResultSetQueryConfig.Return(_config);
        }

        internal MultiIndexedIndices Indices
            => new(_config.temporaryEntityIndices, _config.temporaryFilters, _entityIDs, _count);

        public QueryResult<TResult> Current
            => new(_result, _groups[_groupIndex], Indices);
    }
}
