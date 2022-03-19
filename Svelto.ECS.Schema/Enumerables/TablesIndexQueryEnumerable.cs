using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public readonly ref struct TablesIndexQueryEnumerable<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        internal readonly ResultSetQueryConfig _config;

        internal TablesIndexQueryEnumerable(ResultSetQueryConfig config, IEntityTables<TRow> tables)
        {
            _config = config;

            LoadGroups(tables);
        }

        private void LoadGroups(IEntityTables<TRow> tables)
        {
            int tableCount = tables.Range;

            for (int i = 0; i < tableCount; ++i)
            {
                var table = tables.GetTable(i);

                IterateGroup(table);
            }
        }

        private void IterateGroup(IEntityTable<TRow> table, int groupIndex = 0, int depth = 0)
        {
            if (depth >= table.PrimaryKeys.count)
            {
                var group = table.Group + (uint)groupIndex;
                _config.temporaryGroups.Add(group, group);
                return;
            }

            var pk = table.PrimaryKeys[depth];

            // mutiply parent index
            groupIndex *= pk.possibleKeyCount;

            // when sub-index applied
            if (_config.pkToValue.TryGetValue(pk.id, out var value))
            {
                groupIndex += value;
                IterateGroup(table, groupIndex, depth + 1);
                return;
            }

            // iterate all subgroup
            for (int i = 0; i < pk.possibleKeyCount; ++i)
            {
                IterateGroup(table, groupIndex + i, depth + 1);
            }
        }

        public RefIterator GetEnumerator() => new RefIterator(_config);

        public ref struct RefIterator
        {
            private readonly ResultSetQueryConfig _config;

            private readonly NB<ExclusiveGroupStruct> _groups;
            private readonly uint _groupCount;

            private int _groupIndex;
            private TResult _result;
            private NB<EGIDComponent> _egid;

            internal RefIterator(ResultSetQueryConfig config) : this()
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

                    for (uint i = 0; i < _config.indexers.count; ++i)
                    {
                        if (!_config.indexers[i].groups.TryGetValue(currentGroup, out var groupData) ||
                            groupData.filter.filteredIndices.Count() == 0)
                        {
                            haveAllFilters = false;
                            break;
                        }

                        _config.temporaryFilters.AddAt(i) = groupData.filter;
                    }

                    if (haveAllFilters)
                    {
                        ResultSetHelper<TResult>.Assign(out _result, _config.indexedDB.entitiesDB, currentGroup);

                        if (_config.temporaryFilters.count > 0)
                            (_egid, _) = _config.indexedDB.entitiesDB.QueryEntities<EGIDComponent>(currentGroup);

                        moveNext = true;
                        break;
                    }
                }

                return moveNext;
            }

            public void Reset() { _groupIndex = -1; }

            public void Dispose() { }

            public IndexedQueryResult<TResult, TRow> Current => new IndexedQueryResult<TResult, TRow>(
                _result, new MultiIndexedIndices(_config.temporaryFilters, _egid, _result.count), _groups[_groupIndex]);
        }
    }
}
