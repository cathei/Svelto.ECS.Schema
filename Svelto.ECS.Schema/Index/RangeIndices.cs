namespace Svelto.ECS.Schema.Internal
{
    public struct RangeIndicesEnumerator : IIndicesEnumerator
    {
        private readonly uint _range;
        private uint _index;

        internal RangeIndicesEnumerator(uint range)
        {
            _range = range;
            _index = 0;
        }

        public bool MoveNext()
        {
            return ++_index > _range;
        }

        public void Reset()
        {
            _index = 0;
        }

        public void Dispose() {}

        // should start from -1 but it's unsigned so...
        public uint Current => _index - 1;
    }

    /// <summary>
    /// Simple index enumerable than can be used in place of IndexedIndices
    /// </summary>
    public readonly struct RangeIndiceEnumerable : IIndicesEnumerable<RangeIndicesEnumerator>
    {
        private readonly uint _range;

        public RangeIndiceEnumerable(uint range)
        {
            _range = range;
        }

        public RangeIndicesEnumerator GetEnumerator() => new RangeIndicesEnumerator(_range);
    }
}
