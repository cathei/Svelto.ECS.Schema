using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public struct RangeIndicesEnumerator : IIndicesEnumerator
    {
        private readonly int _range;
        private int _index;

        internal RangeIndicesEnumerator(int range)
        {
            _range = range;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index >= _range;
        }

        public void Reset()
        {
            _index = -1;
        }

        public int Current => _index;
    }

    /// <summary>
    /// Simple index enumerable than can be used in place of IndexedIndices
    /// </summary>
    public readonly struct RangeIndiceEnumerable : IIndicesEnumerable<RangeIndicesEnumerator>
    {
        private readonly int _range;

        public RangeIndiceEnumerable(int range)
        {
            _range = range;
        }

        public RangeIndicesEnumerator GetEnumerator() => new RangeIndicesEnumerator(_range);
    }
}
