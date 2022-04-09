namespace Svelto.ECS.Schema.Internal
{
    public struct RangedIndicesEnumerator
    {
        private readonly uint _range;
        private uint _index;

        internal RangedIndicesEnumerator(uint start, uint range)
        {
            _range = start + range;
            _index = start;
        }

        public bool MoveNext()
        {
            return ++_index <= _range;
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
    public readonly ref struct RangedIndices
    {
        private readonly uint _range;
        private readonly uint _start;

        public RangedIndices(uint range)
        {
            _range = range;
            _start = 0;
        }

        public RangedIndices(uint start, uint range)
        {
            _range = range;
            _start = start;
        }

        public RangedIndicesEnumerator GetEnumerator() => new(_start, _range);
    }
}
