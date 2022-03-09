using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public readonly ref struct TablesQueryEnumerable<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TRow> _tables;

        internal TablesQueryEnumerable(IndexedDB indexedDB, in IEntityTables<TRow> tables)
        {
            _indexedDB = indexedDB;
            _tables = tables;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TRow> _tables;

            private TResult _result;
            private IEntityTable<TRow> _table;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TRow> tables) : this()
            {
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                while (++_indexValue < _tables.Range)
                {
                    _table = _tables.GetTable(_indexValue);

                    if (!_table.ExclusiveGroup.IsEnabled())
                        continue;

                    ResultSetHelper<TResult>.Assign(out _result, _indexedDB.entitiesDB, _table.ExclusiveGroup);
                    if (_result.count == 0)
                        continue;
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; _result = default; }

            public void Dispose() { }

            public QueryResult<TResult, TRow> Current =>
                new QueryResult<TResult, TRow>(_result, _table);
        }
    }
}
