 // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{

    public readonly ref struct IndexQueryTuple<T1, TExtra>
                where T1 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1> _collection;
        private readonly TExtra _extra;

        public IndexQueryTuple(in EntityCollection<T1> collection, in TExtra extra)
        {
            _collection = collection;
            _extra = extra;
        }

        public void Deconstruct(out EntityCollection<T1> collection, out TExtra extra)
        {
            collection = _collection;
            extra = _extra;
        }
    }

    public readonly ref struct IndexQueryTuple<T1, T2, TExtra>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2> _collection;
        private readonly TExtra _extra;

        public IndexQueryTuple(in EntityCollection<T1, T2> collection, in TExtra extra)
        {
            _collection = collection;
            _extra = extra;
        }

        public void Deconstruct(out EntityCollection<T1, T2> collection, out TExtra extra)
        {
            collection = _collection;
            extra = _extra;
        }
    }

    public readonly ref struct IndexQueryTuple<T1, T2, T3, TExtra>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2, T3> _collection;
        private readonly TExtra _extra;

        public IndexQueryTuple(in EntityCollection<T1, T2, T3> collection, in TExtra extra)
        {
            _collection = collection;
            _extra = extra;
        }

        public void Deconstruct(out EntityCollection<T1, T2, T3> collection, out TExtra extra)
        {
            collection = _collection;
            extra = _extra;
        }
    }

    public readonly ref struct IndexQueryTuple<T1, T2, T3, T4, TExtra>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2, T3, T4> _collection;
        private readonly TExtra _extra;

        public IndexQueryTuple(in EntityCollection<T1, T2, T3, T4> collection, in TExtra extra)
        {
            _collection = collection;
            _extra = extra;
        }

        public void Deconstruct(out EntityCollection<T1, T2, T3, T4> collection, out TExtra extra)
        {
            collection = _collection;
            extra = _extra;
        }
    }


    public readonly ref struct IndexQueryTableTuple<TR, T1>
        where TR : class, IEntityRow
                where T1 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1> _collection;
        private readonly IndexedIndices _indices;
        private readonly IEntityTable<TR> _table;

        public IndexQueryTableTuple(in EntityCollection<T1> collection, in IndexedIndices indices, IEntityTable<TR> table)
        {
            _collection = collection;
            _indices = indices;
            _table = table;
        }

        public void Deconstruct(out EntityCollection<T1> collection, out IndexedIndices indices, out IEntityTable<TR> table)
        {
            collection = _collection;
            indices = _indices;
            table = _table;
        }
    }

    public readonly ref struct IndexQueryTableTuple<TR, T1, T2>
        where TR : class, IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2> _collection;
        private readonly IndexedIndices _indices;
        private readonly IEntityTable<TR> _table;

        public IndexQueryTableTuple(in EntityCollection<T1, T2> collection, in IndexedIndices indices, IEntityTable<TR> table)
        {
            _collection = collection;
            _indices = indices;
            _table = table;
        }

        public void Deconstruct(out EntityCollection<T1, T2> collection, out IndexedIndices indices, out IEntityTable<TR> table)
        {
            collection = _collection;
            indices = _indices;
            table = _table;
        }
    }

    public readonly ref struct IndexQueryTableTuple<TR, T1, T2, T3>
        where TR : class, IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2, T3> _collection;
        private readonly IndexedIndices _indices;
        private readonly IEntityTable<TR> _table;

        public IndexQueryTableTuple(in EntityCollection<T1, T2, T3> collection, in IndexedIndices indices, IEntityTable<TR> table)
        {
            _collection = collection;
            _indices = indices;
            _table = table;
        }

        public void Deconstruct(out EntityCollection<T1, T2, T3> collection, out IndexedIndices indices, out IEntityTable<TR> table)
        {
            collection = _collection;
            indices = _indices;
            table = _table;
        }
    }

    public readonly ref struct IndexQueryTableTuple<TR, T1, T2, T3, T4>
        where TR : class, IEntityRow
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2, T3, T4> _collection;
        private readonly IndexedIndices _indices;
        private readonly IEntityTable<TR> _table;

        public IndexQueryTableTuple(in EntityCollection<T1, T2, T3, T4> collection, in IndexedIndices indices, IEntityTable<TR> table)
        {
            _collection = collection;
            _indices = indices;
            _table = table;
        }

        public void Deconstruct(out EntityCollection<T1, T2, T3, T4> collection, out IndexedIndices indices, out IEntityTable<TR> table)
        {
            collection = _collection;
            indices = _indices;
            table = _table;
        }
    }

}