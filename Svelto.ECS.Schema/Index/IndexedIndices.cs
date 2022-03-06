using System;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct IndexedIndicesEnumerator
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

        public uint Current => _indices[_index];
    }

    // To iterate over FilteredIndices with foreach
    public readonly ref struct IndexedIndices
    {
        internal readonly FilteredIndices _indices;

        public int Count() => _indices.Count();

        public uint this[uint index] => _indices[index];
        public uint this[int index] => _indices[index];

        public uint Get(uint index) => _indices[index];

        public IndexedIndices(in FilteredIndices indices)
        {
            _indices = indices;
        }

        public IndexedIndicesEnumerator GetEnumerator() => new IndexedIndicesEnumerator(_indices);
    }
}
