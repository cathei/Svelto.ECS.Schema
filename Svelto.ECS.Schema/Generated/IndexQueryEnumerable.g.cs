 // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{

    public readonly ref struct IndexQueryEnumerable<TR, TIR, T1>
        where TR : IEntityRow<T1>, TIR
        where TIR : IEntityRow
                where T1 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB,
            IEntityTables<TR> tables,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict)
        {
            _indexedDB = indexedDB;
            _tables = tables;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

            private EntityCollection<T1> _collection;
            private FilteredIndices _indices;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                IEntityTables<TR> tables,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict) : this()
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
                    var table = _tables.GetTable(_indexValue);

                    if (!_dict.TryGetValue(table.ExclusiveGroup, out var groupData))
                        continue;

                    _indices = groupData.filter.filteredIndices;

                    if (!table.ExclusiveGroup.IsEnabled() || _indices.Count() == 0)
                        continue;

                    _collection = _indexedDB.Select<TR>().From(table).Entities();
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryTableTuple<TR, T1> Current => new IndexQueryTableTuple<TR, T1>(
                _collection, new IndexedIndices(_indices), _tables.GetTable(_indexValue));
        }
    }

    public readonly ref struct IndexQueryEnumerable<TR, TIR, T1, T2>
        where TR : IEntityRow<T1, T2>, TIR
        where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB,
            IEntityTables<TR> tables,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict)
        {
            _indexedDB = indexedDB;
            _tables = tables;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

            private EntityCollection<T1, T2> _collection;
            private FilteredIndices _indices;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                IEntityTables<TR> tables,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict) : this()
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
                    var table = _tables.GetTable(_indexValue);

                    if (!_dict.TryGetValue(table.ExclusiveGroup, out var groupData))
                        continue;

                    _indices = groupData.filter.filteredIndices;

                    if (!table.ExclusiveGroup.IsEnabled() || _indices.Count() == 0)
                        continue;

                    _collection = _indexedDB.Select<TR>().From(table).Entities();
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryTableTuple<TR, T1, T2> Current => new IndexQueryTableTuple<TR, T1, T2>(
                _collection, new IndexedIndices(_indices), _tables.GetTable(_indexValue));
        }
    }

    public readonly ref struct IndexQueryEnumerable<TR, TIR, T1, T2, T3>
        where TR : IEntityRow<T1, T2, T3>, TIR
        where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB,
            IEntityTables<TR> tables,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict)
        {
            _indexedDB = indexedDB;
            _tables = tables;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

            private EntityCollection<T1, T2, T3> _collection;
            private FilteredIndices _indices;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                IEntityTables<TR> tables,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict) : this()
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
                    var table = _tables.GetTable(_indexValue);

                    if (!_dict.TryGetValue(table.ExclusiveGroup, out var groupData))
                        continue;

                    _indices = groupData.filter.filteredIndices;

                    if (!table.ExclusiveGroup.IsEnabled() || _indices.Count() == 0)
                        continue;

                    _collection = _indexedDB.Select<TR>().From(table).Entities();
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryTableTuple<TR, T1, T2, T3> Current => new IndexQueryTableTuple<TR, T1, T2, T3>(
                _collection, new IndexedIndices(_indices), _tables.GetTable(_indexValue));
        }
    }

    public readonly ref struct IndexQueryEnumerable<TR, TIR, T1, T2, T3, T4>
        where TR : IEntityRow<T1, T2, T3, T4>, TIR
        where TIR : IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB,
            IEntityTables<TR> tables,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict)
        {
            _indexedDB = indexedDB;
            _tables = tables;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

            private EntityCollection<T1, T2, T3, T4> _collection;
            private FilteredIndices _indices;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                IEntityTables<TR> tables,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict) : this()
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
                    var table = _tables.GetTable(_indexValue);

                    if (!_dict.TryGetValue(table.ExclusiveGroup, out var groupData))
                        continue;

                    _indices = groupData.filter.filteredIndices;

                    if (!table.ExclusiveGroup.IsEnabled() || _indices.Count() == 0)
                        continue;

                    _collection = _indexedDB.Select<TR>().From(table).Entities();
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryTableTuple<TR, T1, T2, T3, T4> Current => new IndexQueryTableTuple<TR, T1, T2, T3, T4>(
                _collection, new IndexedIndices(_indices), _tables.GetTable(_indexValue));
        }
    }

}