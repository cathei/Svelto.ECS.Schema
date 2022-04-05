using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct SelectFromQueryEnumerator<TRow, TResult>
        where TRow : class, IEntityRow
        where TResult : struct
    {
        private readonly ResultSetQueryConfig _config;
        private readonly SelectFromQueryDelegate<TResult> _selector;

        private readonly NB<ExclusiveGroupStruct> _groups;
        private readonly uint _groupCount;

        private int _groupIndex;
        private TResult _result;
        private NativeEntityIDs _entityIDs;
        private int _count;

        internal SelectFromQueryEnumerator(ResultSetQueryConfig config, SelectFromQueryDelegate<TResult> selector) : this()
        {
            _config = config;
            _selector = selector;
            _groupIndex = -1;

            _groups = _config.temporaryGroups.GetValues(out _groupCount);
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
                    (_entityIDs, _count) = _config.indexedDB.QueryEntityIDs(currentGroup);

                    if (_count == 0)
                        continue;

                    _result = _selector(_config.indexedDB.entitiesDB, currentGroup);
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

        public SelectFromQueryResult<TResult> Current =>
            new(_result, _groups[_groupIndex], new(_config.temporaryFilters, _entityIDs, _count));
    }
}
