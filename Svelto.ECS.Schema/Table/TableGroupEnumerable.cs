using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    internal readonly ref struct TableGroupEnumerable<TRow>
        where TRow : class, IEntityRow
    {
        internal readonly IEntityTables<TRow> _table;
        internal readonly FasterDictionary<int, int> _pkIdToValue;

        public TableGroupEnumerable(IEntityTables<TRow> table, FasterDictionary<int, int> pkIdToValue)
        {
            _table = table;
            _pkIdToValue = pkIdToValue;
        }

        public RefIterator GetEnumerator() => new RefIterator(_table, _pkIdToValue);

        public ref struct RefIterator
        {
            internal readonly IEntityTables<TRow> _table;
            internal readonly FasterDictionary<int, int> _pkIdToValue;

            internal int _tableIndex;
            internal int _lastGroupIndex;

            internal IEntityTable<TRow> _currentTable;
            internal LocalFasterReadOnlyList<Table.PrimaryKeyInfo> _currentPrimaryKeys;
            internal ExclusiveGroupStruct _group;

            public RefIterator(IEntityTables<TRow> table, FasterDictionary<int, int> pkIdToValue) : this()
            {
                _table = table;
                _pkIdToValue = pkIdToValue;

                _tableIndex = -1;
                _lastGroupIndex = 0;
            }

            public bool MoveNext()
            {
                if (_currentTable == null || _lastGroupIndex >= _currentTable.GroupRange)
                {
                    if (++_tableIndex >= _table.Range)
                        return false;

                    _currentTable = _table.GetTable(_tableIndex);
                    _currentPrimaryKeys = _currentTable.PrimaryKeys;
                    _lastGroupIndex = 0;
                }

                int groupIndex = 0;

                for (int i = 0; i < _currentPrimaryKeys.count; ++i)
                {
                    var info = _currentPrimaryKeys[i];

                    // mutiply parent index
                    groupIndex *= info.possibleKeyCount;

                    if (_pkIdToValue.TryGetValue(info.id, out int value))
                    {
                        // give offset
                        groupIndex += value;
                        // count as looping whole groups for this pk
                        _lastGroupIndex += info.possibleKeyCount;
                    }
                }

                _group = _currentTable.Group + (uint)groupIndex;
                return true;
            }

            public void Reset()
            {
                _currentTable = null;
                _currentPrimaryKeys = null;

                _tableIndex = -1;
                _lastGroupIndex = -1;
            }

            public void Dispose() { }

            public (IEntityTable<TRow>, ExclusiveGroupStruct) Current => (_currentTable, _group);
        }
    }
}