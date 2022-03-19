using System;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct MultiIndexedIndicesEnumerator
    {
        private readonly NativeDynamicArrayCast<FilterGroup> _filters;
        private readonly NB<EGIDComponent> _egid;

        private int _index;
        private uint _current;

        private readonly int _maxCount;
        private FilteredIndices _indices;

        internal MultiIndexedIndicesEnumerator(
            NativeDynamicArrayCast<FilterGroup> filters, NB<EGIDComponent> egid, int count) : this()
        {
            _filters = filters;
            _egid = egid;

            _index = -1;
            _current = 0;

            if (filters.count > 0)
            {
                _indices = filters[0].filteredIndices;
                _maxCount = _indices.Count();
            }
            else
            {
                _maxCount = count;
            }
        }

        public bool MoveNext()
        {
            while (++_index < _maxCount)
            {
                if (_filters.count > 0)
                {
                    _current = _indices[_index];

                    bool haveAllFilters = true;

                    for (int i = 1; i < _filters.count; ++i)
                    {
                        if (_filters[i].Exists(_egid[_current].ID.entityID))
                        {
                            haveAllFilters = false;
                            break;
                        }
                    }

                    if (!haveAllFilters)
                        continue;

                    return true;
                }
                else
                {
                    _current = (uint)_index;
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose() { }

        public uint Current => _current;
    }

    // To iterate over FilteredIndices with foreach
    public readonly ref struct MultiIndexedIndices
    {
        private readonly NativeDynamicArrayCast<FilterGroup> _filters;
        private readonly NB<EGIDComponent> _egid;
        private readonly int _count;

        public MultiIndexedIndices(
            NativeDynamicArrayCast<FilterGroup> filters, NB<EGIDComponent> egid, int count)
        {
            _filters = filters;
            _egid = egid;
            _count = count;
        }

        /// <summary>
        /// NOTE: If there is no filters it count as existing ID
        /// </summary>
        internal bool Exists(uint entityID)
        {
            for (int i = 0; i < _filters.count; ++i)
            {
                if (!_filters[i].Exists(entityID))
                    return false;
            }

            return true;
        }

        public MultiIndexedIndicesEnumerator GetEnumerator() =>
            new MultiIndexedIndicesEnumerator(_filters, _egid, _count);
    }
}
