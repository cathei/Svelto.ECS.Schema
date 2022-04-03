using Svelto.DataStructures;
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
        private NB<EGIDComponent> _egid;
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

                for (int i = 0; i < _config.indexers.count; ++i)
                {
                    var keyData = _config.indexers[i];

                    if (keyData.groups == null ||
                        !keyData.groups.TryGetValue(currentGroup, out var groupData) ||
                        groupData.filter.filteredIndices.Count() == 0)
                    {
                        haveAllFilters = false;
                        break;
                    }

                    _config.temporaryFilters.AddAt((uint)i) = groupData.filter;
                }

                if (haveAllFilters)
                {
                    (_egid, _count) = _config.indexedDB.entitiesDB.QueryEntities<EGIDComponent>(currentGroup);
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
            new(_config, _groups[_groupIndex], new(_config.temporaryFilters, _egid, _count));
    }
}
