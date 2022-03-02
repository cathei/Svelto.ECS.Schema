 // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{

    public readonly ref struct IndexQueryEnumerable<T1>
                where T1 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict)
        {
            _indexedDB = indexedDB;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly MB<IndexedGroupData> _dictValues;
            private readonly uint _count;

            private EntityCollection<T1> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict) : this()
            {
                _indexedDB = indexedDB;
                _indexValue = -1;

                if (dict != null)
                    _dictValues = dict.GetValues(out _count);
            }

            public bool MoveNext()
            {
                while (++_indexValue < _count)
                {
                    _groupData = _dictValues[_indexValue];

                    if (!_groupData.group.IsEnabled())
                        continue;

                    EntityCollection<T1> collection = _indexedDB.entitiesDB.QueryEntities<T1>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1> Current => new IndexQueryGroupTuple<T1>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

    public readonly ref struct IndexQueryEnumerable<T1, T2>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict)
        {
            _indexedDB = indexedDB;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly MB<IndexedGroupData> _dictValues;
            private readonly uint _count;

            private EntityCollection<T1, T2> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict) : this()
            {
                _indexedDB = indexedDB;
                _indexValue = -1;

                if (dict != null)
                    _dictValues = dict.GetValues(out _count);
            }

            public bool MoveNext()
            {
                while (++_indexValue < _count)
                {
                    _groupData = _dictValues[_indexValue];

                    if (!_groupData.group.IsEnabled())
                        continue;

                    EntityCollection<T1, T2> collection = _indexedDB.entitiesDB.QueryEntities<T1, T2>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1, T2> Current => new IndexQueryGroupTuple<T1, T2>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

    public readonly ref struct IndexQueryEnumerable<T1, T2, T3>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict)
        {
            _indexedDB = indexedDB;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly MB<IndexedGroupData> _dictValues;
            private readonly uint _count;

            private EntityCollection<T1, T2, T3> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict) : this()
            {
                _indexedDB = indexedDB;
                _indexValue = -1;

                if (dict != null)
                    _dictValues = dict.GetValues(out _count);
            }

            public bool MoveNext()
            {
                while (++_indexValue < _count)
                {
                    _groupData = _dictValues[_indexValue];

                    if (!_groupData.group.IsEnabled())
                        continue;

                    EntityCollection<T1, T2, T3> collection = _indexedDB.entitiesDB.QueryEntities<T1, T2, T3>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1, T2, T3> Current => new IndexQueryGroupTuple<T1, T2, T3>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

    public readonly ref struct IndexQueryEnumerable<T1, T2, T3, T4>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict)
        {
            _indexedDB = indexedDB;
            _dict = dict;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly MB<IndexedGroupData> _dictValues;
            private readonly uint _count;

            private EntityCollection<T1, T2, T3, T4> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict) : this()
            {
                _indexedDB = indexedDB;
                _indexValue = -1;

                if (dict != null)
                    _dictValues = dict.GetValues(out _count);
            }

            public bool MoveNext()
            {
                while (++_indexValue < _count)
                {
                    _groupData = _dictValues[_indexValue];

                    if (!_groupData.group.IsEnabled())
                        continue;

                    EntityCollection<T1, T2, T3, T4> collection = _indexedDB.entitiesDB.QueryEntities<T1, T2, T3, T4>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1, T2, T3, T4> Current => new IndexQueryGroupTuple<T1, T2, T3, T4>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }


    public readonly ref struct IndexQueryGroupsEnumerable<T1>
                where T1 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        internal IndexQueryGroupsEnumerable(IndexedDB indexedDB,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
            in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _indexedDB = indexedDB;
            _dict = dict;
            _groups = groups;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict, _groups);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
            private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            private EntityCollection<T1> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _indexedDB = indexedDB;
                _dict = dict;
                _groups = groups;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                if (_dict == null)
                    return false;

                while (++_indexValue < _groups.count)
                {
                    if (!_dict.TryGetValue(_groups[_indexValue], out _groupData))
                        continue;

                    var indices = _groupData.filter.filteredIndices;

                    if (!_groupData.group.IsEnabled() || indices.Count() == 0)
                        continue;

                    EntityCollection<T1> collection = _indexedDB.entitiesDB.QueryEntities<T1>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _groups.count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1> Current => new IndexQueryGroupTuple<T1>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

    public readonly ref struct IndexQueryGroupsEnumerable<T1, T2>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        internal IndexQueryGroupsEnumerable(IndexedDB indexedDB,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
            in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _indexedDB = indexedDB;
            _dict = dict;
            _groups = groups;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict, _groups);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
            private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            private EntityCollection<T1, T2> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _indexedDB = indexedDB;
                _dict = dict;
                _groups = groups;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                if (_dict == null)
                    return false;

                while (++_indexValue < _groups.count)
                {
                    if (!_dict.TryGetValue(_groups[_indexValue], out _groupData))
                        continue;

                    var indices = _groupData.filter.filteredIndices;

                    if (!_groupData.group.IsEnabled() || indices.Count() == 0)
                        continue;

                    EntityCollection<T1, T2> collection = _indexedDB.entitiesDB.QueryEntities<T1, T2>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _groups.count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1, T2> Current => new IndexQueryGroupTuple<T1, T2>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

    public readonly ref struct IndexQueryGroupsEnumerable<T1, T2, T3>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        internal IndexQueryGroupsEnumerable(IndexedDB indexedDB,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
            in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _indexedDB = indexedDB;
            _dict = dict;
            _groups = groups;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict, _groups);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
            private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            private EntityCollection<T1, T2, T3> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _indexedDB = indexedDB;
                _dict = dict;
                _groups = groups;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                if (_dict == null)
                    return false;

                while (++_indexValue < _groups.count)
                {
                    if (!_dict.TryGetValue(_groups[_indexValue], out _groupData))
                        continue;

                    var indices = _groupData.filter.filteredIndices;

                    if (!_groupData.group.IsEnabled() || indices.Count() == 0)
                        continue;

                    EntityCollection<T1, T2, T3> collection = _indexedDB.entitiesDB.QueryEntities<T1, T2, T3>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _groups.count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1, T2, T3> Current => new IndexQueryGroupTuple<T1, T2, T3>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

    public readonly ref struct IndexQueryGroupsEnumerable<T1, T2, T3, T4>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
        private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        internal IndexQueryGroupsEnumerable(IndexedDB indexedDB,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
            in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _indexedDB = indexedDB;
            _dict = dict;
            _groups = groups;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _dict, _groups);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> _dict;
            private readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            private EntityCollection<T1, T2, T3, T4> _collection;
            private IndexedGroupData _groupData;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData> dict,
                in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _indexedDB = indexedDB;
                _dict = dict;
                _groups = groups;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                if (_dict == null)
                    return false;

                while (++_indexValue < _groups.count)
                {
                    if (!_dict.TryGetValue(_groups[_indexValue], out _groupData))
                        continue;

                    var indices = _groupData.filter.filteredIndices;

                    if (!_groupData.group.IsEnabled() || indices.Count() == 0)
                        continue;

                    EntityCollection<T1, T2, T3, T4> collection = _indexedDB.entitiesDB.QueryEntities<T1, T2, T3, T4>(_groupData.group);

                    if (_groupData.filter.filteredIndices.Count() == 0)
                        continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _groups.count;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public IndexQueryGroupTuple<T1, T2, T3, T4> Current => new IndexQueryGroupTuple<T1, T2, T3, T4>(
                _collection, new IndexedIndices(_groupData.filter.filteredIndices), _groupData.group);
        }
    }

}