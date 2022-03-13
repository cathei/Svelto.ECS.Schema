using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public readonly ref struct TablesIndexQueryEnumerable<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        private readonly ResultSetQueryConfig _config;
        private readonly IEntityTables<TRow> _tables;

        internal TablesIndexQueryEnumerable(ResultSetQueryConfig config, IEntityTables<TRow> tables)
        {
            _config = config;
            _tables = tables;
        }

        public RefIterator GetEnumerator() => new RefIterator(_config, _tables);

        public ref struct RefIterator
        {
            private readonly ResultSetQueryConfig _config;
            private readonly IEntityTables<TRow> _tables;

            private int _indexValue;
            private TableGroupEnumerable.RefIterator _tableIter;

            private TResult _result;
            private FilteredIndices _indices;

            internal RefIterator(ResultSetQueryConfig config, IEntityTables<TRow> tables) : this()
            {
                _config = config;
                _tables = tables;

                _dict = dict;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                if (_dict == null)
                    return false;

                while (++_indexValue < _tables.Range)
                {
                    _table = _tables.GetTable(_indexValue);

                    if (!_dict.TryGetValue(_table.ExclusiveGroup, out var groupData))
                        continue;

                    _indices = groupData.filter.filteredIndices;

                    if (!_table.ExclusiveGroup.IsEnabled() || _indices.Count() == 0)
                        continue;

                    ResultSetHelper<TResult>.Assign(out _result, _indexedDB.entitiesDB, _table.ExclusiveGroup);
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; _result = default; }

            public void Dispose() { }

            public IndexedQueryResult<TResult, TRow> Current =>
                new IndexedQueryResult<TResult, TRow>(_result, new IndexedIndices(_indices), _table);
        }
    }
}
