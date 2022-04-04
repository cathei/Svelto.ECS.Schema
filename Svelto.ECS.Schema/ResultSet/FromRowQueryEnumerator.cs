using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct FromRowQueryEnumerator<TRow>
        where TRow : class, IEntityRow
    {
        private readonly ResultSetQueryConfig _config;

        private readonly NB<ExclusiveGroupStruct> _groups;
        private readonly uint _groupCount;

        private int _groupIndex;
        private NativeEntityIDs _entityIDs;
        private int _count;

        internal FromRowQueryEnumerator(ResultSetQueryConfig config) : this()
        {
            _config = config;
            _groupIndex = -1;

            _groups = _config.temporaryGroups.GetValues(out _groupCount);
        }

        public bool MoveNext()
        {
            bool moveNext = false;

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
                    (_, _entityIDs, _count) = _config.indexedDB.entitiesDB.QueryEntities<RowIdentityComponent>(currentGroup);
                    moveNext = true;
                    break;
                }
            }

            return moveNext;
        }

        public void Reset() { _groupIndex = -1; }

        public void Dispose()
        {
            // TODO: auto-close option might be needed
            ResultSetQueryConfig.Return(_config);
        }

        public FromGroupQuery<TRow> Current =>
            new(_config, _groups[_groupIndex], new(_config.temporaryFilters, _entityIDs, _count));
    }
}
