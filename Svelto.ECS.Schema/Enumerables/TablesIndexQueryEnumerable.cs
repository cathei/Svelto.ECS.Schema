using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public readonly ref struct TablesIndexQueryEnumerable<TResult, TRow>
        where TResult : struct, IResultSet
        where TRow : class, IEntityRow
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TRow> _tables;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> _dict;

        internal TablesIndexQueryEnumerable(IndexedDB indexedDB,
            IEntityTables<TRow> tables,
            FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> dict)
        {
            _indexedDB = indexedDB;
            _tables = tables;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TRow> _tables;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> _dict;

            private TResult _result;
            private FilteredIndices _indices;
            private IEntityTable<TRow> _table;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                IEntityTables<TRow> tables,
                FasterDictionary<ExclusiveGroupStruct, IndexerGroupData> dict) : this()
            {
                _indexedDB = indexedDB;
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
