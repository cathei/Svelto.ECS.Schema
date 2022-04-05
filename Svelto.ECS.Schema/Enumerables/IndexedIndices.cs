namespace Svelto.ECS.Schema
{
    public ref struct IndexedIndicesEnumerator
    {
        private readonly EntityFilterIndices _indices;
        private int _index;

        internal IndexedIndicesEnumerator(in EntityFilterIndices indices)
        {
            _indices = indices;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _indices.count;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose() { }

        public uint Current => _indices[_index];
    }

    // To iterate over EntityFilterIndices with foreach
    public readonly ref struct IndexedIndices
    {
        internal readonly EntityFilterIndices _indices;

        public uint count => _indices.count;

        public uint this[uint index] => _indices[index];
        public uint this[int index] => _indices[index];

        public uint Get(uint index) => _indices[index];

        public IndexedIndices(in EntityFilterIndices indices)
        {
            _indices = indices;
        }

        public IndexedIndicesEnumerator GetEnumerator() => new(_indices);
    }
}
