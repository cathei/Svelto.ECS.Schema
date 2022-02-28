using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    // IDisposable is required here to ensure zero-allocation
    // Because compiler has to be able to see Dispose() method
    // https://ericlippert.com/2011/03/14/to-box-or-not-to-box/
    public interface IIndicesEnumerator : IDisposable
    {
        bool MoveNext();
        void Reset();
        int Current { get; }
    }

    public interface IIndicesEnumerable<TIter>
        where TIter : struct, IIndicesEnumerator
    {
        TIter GetEnumerator();
    }
}

namespace Svelto.ECS.Schema
{
    public struct IndexedIndicesEnumerator : IIndicesEnumerator
    {
        private readonly FilteredIndices _indices;
        private int _index;

        internal IndexedIndicesEnumerator(in FilteredIndices indices)
        {
            _indices = indices;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _indices.Count();
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose() { }

        public int Current => (int)_indices[_index];
    }

    // To iterate over FilteredIndices with foreach
    public readonly struct IndexedIndices : IIndicesEnumerable<IndexedIndicesEnumerator>
    {
        private readonly FilteredIndices _indices;

        public int Count() => _indices.Count();

        public uint this[uint index] => _indices[index];
        public uint this[int index] => _indices[index];

        public uint Get(uint index) => _indices[index];

        public IndexedIndices(FilteredIndices indices)
        {
            _indices = indices;
        }

        public IndexedIndicesEnumerator GetEnumerator() => new IndexedIndicesEnumerator(_indices);
    }
}
