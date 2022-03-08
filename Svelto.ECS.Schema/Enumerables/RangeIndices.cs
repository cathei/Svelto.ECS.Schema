namespace Svelto.ECS.Schema.Internal
{
    public struct RangedIndicesEnumerator
    {
        private readonly uint _range;
        private uint _index;

        internal RangedIndicesEnumerator(uint range)
        {
            _range = range;
            _index = 0;
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

        public RangedIndices(uint range)
        {
            _range = range;
        }

        public RangedIndicesEnumerator GetEnumerator() => new RangedIndicesEnumerator(_range);
    }
}
