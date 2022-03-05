 // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{

    public readonly ref struct TablesEnumerable<TR, T1>
        where TR : ISelectorRow<T1>
                where T1 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;

        internal TablesEnumerable(IndexedDB indexedDB, in IEntityTables<TR> tables)
        {
            _indexedDB = indexedDB;
            _tables = tables;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;

            private EntityCollection<T1> _collection;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TR> tables) : this()
            {
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                while (++_indexValue < _tables.Range)
                {
                    var group = _tables.GetTable(_indexValue).ExclusiveGroup;

                    if (!group.IsEnabled())
                        continue;

                    var collection = _indexedDB.entitiesDB.QueryEntities<T1>(group);

                    // cannot do this due because count is internal...
                    // if (collection.count == 0)
                    //     continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public RefCurrent Current => new RefCurrent(_collection, _tables.GetTable(_indexValue));

            public readonly ref struct RefCurrent
            {
                public readonly EntityCollection<T1> _buffers;
                public readonly IEntityTable<TR> _table;

                public RefCurrent(in EntityCollection<T1> buffers, IEntityTable<TR> table)
                {
                    _buffers = buffers;
                    _table = table;
                }

                public void Deconstruct(out EntityCollection<T1> buffers, out IEntityTable<TR> table)
                {
                    buffers = _buffers;
                    table = _table;
                }
            }
        }
    }

    public readonly ref struct TablesEnumerable<TR, T1, T2>
        where TR : ISelectorRow<T1, T2>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;

        internal TablesEnumerable(IndexedDB indexedDB, in IEntityTables<TR> tables)
        {
            _indexedDB = indexedDB;
            _tables = tables;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;

            private EntityCollection<T1, T2> _collection;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TR> tables) : this()
            {
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                while (++_indexValue < _tables.Range)
                {
                    var group = _tables.GetTable(_indexValue).ExclusiveGroup;

                    if (!group.IsEnabled())
                        continue;

                    var collection = _indexedDB.entitiesDB.QueryEntities<T1, T2>(group);

                    // cannot do this due because count is internal...
                    // if (collection.count == 0)
                    //     continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public RefCurrent Current => new RefCurrent(_collection, _tables.GetTable(_indexValue));

            public readonly ref struct RefCurrent
            {
                public readonly EntityCollection<T1, T2> _buffers;
                public readonly IEntityTable<TR> _table;

                public RefCurrent(in EntityCollection<T1, T2> buffers, IEntityTable<TR> table)
                {
                    _buffers = buffers;
                    _table = table;
                }

                public void Deconstruct(out EntityCollection<T1, T2> buffers, out IEntityTable<TR> table)
                {
                    buffers = _buffers;
                    table = _table;
                }
            }
        }
    }

    public readonly ref struct TablesEnumerable<TR, T1, T2, T3>
        where TR : ISelectorRow<T1, T2, T3>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;

        internal TablesEnumerable(IndexedDB indexedDB, in IEntityTables<TR> tables)
        {
            _indexedDB = indexedDB;
            _tables = tables;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;

            private EntityCollection<T1, T2, T3> _collection;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TR> tables) : this()
            {
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                while (++_indexValue < _tables.Range)
                {
                    var group = _tables.GetTable(_indexValue).ExclusiveGroup;

                    if (!group.IsEnabled())
                        continue;

                    var collection = _indexedDB.entitiesDB.QueryEntities<T1, T2, T3>(group);

                    // cannot do this due because count is internal...
                    // if (collection.count == 0)
                    //     continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public RefCurrent Current => new RefCurrent(_collection, _tables.GetTable(_indexValue));

            public readonly ref struct RefCurrent
            {
                public readonly EntityCollection<T1, T2, T3> _buffers;
                public readonly IEntityTable<TR> _table;

                public RefCurrent(in EntityCollection<T1, T2, T3> buffers, IEntityTable<TR> table)
                {
                    _buffers = buffers;
                    _table = table;
                }

                public void Deconstruct(out EntityCollection<T1, T2, T3> buffers, out IEntityTable<TR> table)
                {
                    buffers = _buffers;
                    table = _table;
                }
            }
        }
    }

    public readonly ref struct TablesEnumerable<TR, T1, T2, T3, T4>
        where TR : ISelectorRow<T1, T2, T3, T4>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;

        internal TablesEnumerable(IndexedDB indexedDB, in IEntityTables<TR> tables)
        {
            _indexedDB = indexedDB;
            _tables = tables;
        }

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;

            private EntityCollection<T1, T2, T3, T4> _collection;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TR> tables) : this()
            {
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }

            public bool MoveNext()
            {
                while (++_indexValue < _tables.Range)
                {
                    var group = _tables.GetTable(_indexValue).ExclusiveGroup;

                    if (!group.IsEnabled())
                        continue;

                    var collection = _indexedDB.entitiesDB.QueryEntities<T1, T2, T3, T4>(group);

                    // cannot do this due because count is internal...
                    // if (collection.count == 0)
                    //     continue;

                    _collection = collection;
                    break;
                }

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexValue = -1; }

            public RefCurrent Current => new RefCurrent(_collection, _tables.GetTable(_indexValue));

            public readonly ref struct RefCurrent
            {
                public readonly EntityCollection<T1, T2, T3, T4> _buffers;
                public readonly IEntityTable<TR> _table;

                public RefCurrent(in EntityCollection<T1, T2, T3, T4> buffers, IEntityTable<TR> table)
                {
                    _buffers = buffers;
                    _table = table;
                }

                public void Deconstruct(out EntityCollection<T1, T2, T3, T4> buffers, out IEntityTable<TR> table)
                {
                    buffers = _buffers;
                    table = _table;
                }
            }
        }
    }

}