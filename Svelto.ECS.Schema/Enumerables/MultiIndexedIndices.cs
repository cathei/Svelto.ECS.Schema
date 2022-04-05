using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct MultiIndexedIndicesEnumerator
    {
        private readonly NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> _filters;
        private readonly NativeEntityIDs _entityIDs;

        private int _index;
        private uint _current;

        private readonly int _maxCount;
        private EntityFilterIndices _indices;

        internal MultiIndexedIndicesEnumerator(
            NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> filters, NativeEntityIDs entityIDs, int count) : this()
        {
            _filters = filters;
            _entityIDs = entityIDs;

            _index = -1;
            _current = 0;

            if (filters.count > 0)
            {
                _indices = filters[0].indices;
                _maxCount = (int)_indices.count;
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

                    bool entityIsInAllFilters = true;

                    for (int i = 1; i < _filters.count; ++i)
                    {
                        if (_filters[i].Exists(_entityIDs[_current]))
                        {
                            entityIsInAllFilters = false;
                            break;
                        }
                    }

                    if (!entityIsInAllFilters)
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
        private readonly NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> _filters;
        internal readonly NativeEntityIDs _entityIDs;
        private readonly int _count;

        public MultiIndexedIndices(
            NativeDynamicArrayCast<EntityFilterCollection.GroupFilters> filters, NativeEntityIDs entityIDs, int count)
        {
            _filters = filters;
            _entityIDs = entityIDs;
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

        public MultiIndexedIndicesEnumerator GetEnumerator() => new(_filters, _entityIDs, _count);
    }
}
