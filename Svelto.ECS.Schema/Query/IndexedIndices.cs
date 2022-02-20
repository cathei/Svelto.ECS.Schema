using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Schema
{
    public ref struct FilteredIndicesEnumerator
    {
        private readonly FilteredIndices _indices;
        private int _index;

        internal FilteredIndicesEnumerator(in FilteredIndices indices)
        {
            _indices = indices;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _indices.Count();
        }

        public uint Current => _indices[_index];
    }

    // To iterate over FilteredIndices with foreach
    public readonly struct IndexedIndices
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

        public FilteredIndicesEnumerator GetEnumerator() => new FilteredIndicesEnumerator(_indices);
    }
}