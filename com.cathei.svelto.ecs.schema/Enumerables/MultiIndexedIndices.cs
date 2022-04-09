using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct MultiIndexedIndicesEnumerator
    {
        private readonly NativeDynamicArrayCast<uint> _selectedIndices;
        private readonly NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> _filters;
        private readonly NativeEntityIDs _entityIDs;

        private int _index;
        internal uint _current;

        private readonly int _maxCount;

        internal MultiIndexedIndicesEnumerator(
            NativeDynamicArrayCast<uint> selectedIndices,
            NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> filters,
            NativeEntityIDs entityIDs, int count) : this()
        {
            _selectedIndices = selectedIndices;
            _filters = filters;
            _entityIDs = entityIDs;

            _index = -1;
            _current = 0;

            if (_selectedIndices.count > 0)
            {
                _maxCount = _selectedIndices.count;
            }
            else if (_filters.count > 0)
            {
                _maxCount = (int)_filters[0].count;
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
                if (_selectedIndices.count > 0)
                {
                    _current = _selectedIndices[_index];

                    if (!ExistInAllFilters(_entityIDs[_current], 0))
                        continue;

                    return true;

                }
                else if (_filters.count > 0)
                {
                    // I prefer iterating it reverse
                    // because it will make it safe to alter filters
                    _current =_filters[0]._entityIDToDenseIndex
                        .unsafeValues[_maxCount - _index - 1];

                    if (!ExistInAllFilters(_entityIDs[_current], 1))
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

        private bool ExistInAllFilters(uint entityID, int filterStart)
        {
            for (int i = filterStart; i < _filters.count; ++i)
            {
                if (!_filters[i].Exists(entityID))
                    return false;
            }

            return true;
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
        private readonly NativeDynamicArrayCast<uint> _selectedIndices;
        private readonly NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> _filters;
        internal readonly NativeEntityIDs _entityIDs;
        private readonly int _count;

        public MultiIndexedIndices(
            NativeDynamicArrayCast<uint> selectedIndices,
            NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> filters,
            NativeEntityIDs entityIDs, int count)
        {
            _selectedIndices = selectedIndices;
            _filters = filters;
            _entityIDs = entityIDs;
            _count = count;
        }

        internal bool IndexExists(uint index)
        {
            if (index >= _count)
                return false;

            if (_selectedIndices.count > 0)
            {
                bool matchAny = false;

                for (int i = 0; i < _selectedIndices.count; ++i)
                {
                    if (_selectedIndices[i] == index)
                    {
                        matchAny = true;
                        break;
                    }
                }

                if (!matchAny)
                    return false;
            }

            uint entityID = _entityIDs[index];

            for (int i = 0; i < _filters.count; ++i)
            {
                if (!_filters[i].Exists(entityID))
                    return false;
            }

            return true;
        }

        public MultiIndexedIndicesEnumerator GetEnumerator()
            => new(_selectedIndices, _filters, _entityIDs, _count);
    }
}
