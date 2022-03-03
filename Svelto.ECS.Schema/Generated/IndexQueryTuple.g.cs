 // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{

    public readonly ref struct IndexQueryTuple<T1>
                where T1 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1> _collection;
        private readonly IndexedIndices _indices;

        public IndexQueryTuple(in EntityCollection<T1> collection, in IndexedIndices indices)
        {
            _collection = collection;
            _indices = indices;
        }

        public void Deconstruct(out EntityCollection<T1> collection, out IndexedIndices indices)
        {
            collection = _collection;
            indices = _indices;
        }
    }

    public readonly ref struct IndexQueryTuple<T1, T2>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2> _collection;
        private readonly IndexedIndices _indices;

        public IndexQueryTuple(in EntityCollection<T1, T2> collection, in IndexedIndices indices)
        {
            _collection = collection;
            _indices = indices;
        }

        public void Deconstruct(out EntityCollection<T1, T2> collection, out IndexedIndices indices)
        {
            collection = _collection;
            indices = _indices;
        }
    }

    public readonly ref struct IndexQueryTuple<T1, T2, T3>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2, T3> _collection;
        private readonly IndexedIndices _indices;

        public IndexQueryTuple(in EntityCollection<T1, T2, T3> collection, in IndexedIndices indices)
        {
            _collection = collection;
            _indices = indices;
        }

        public void Deconstruct(out EntityCollection<T1, T2, T3> collection, out IndexedIndices indices)
        {
            collection = _collection;
            indices = _indices;
        }
    }

    public readonly ref struct IndexQueryTuple<T1, T2, T3, T4>
                where T1 : struct, IEntityComponent
                where T2 : struct, IEntityComponent
                where T3 : struct, IEntityComponent
                where T4 : struct, IEntityComponent
    {
        private readonly EntityCollection<T1, T2, T3, T4> _collection;
        private readonly IndexedIndices _indices;

        public IndexQueryTuple(in EntityCollection<T1, T2, T3, T4> collection, in IndexedIndices indices)
        {
            _collection = collection;
            _indices = indices;
        }

        public void Deconstruct(out EntityCollection<T1, T2, T3, T4> collection, out IndexedIndices indices)
        {
            collection = _collection;
            indices = _indices;
        }
    }


    public readonly ref struct IndexQueryTableTuple<TR, T1>
        where TR : IEntityRow
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
        where TR : IEntityRow
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
        where TR : IEntityRow
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
        where TR : IEntityRow
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