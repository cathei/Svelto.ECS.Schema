using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct JoinedIndexedIndicesEnumerator<T1>
        where T1 : unmanaged, IForeignKeyComponent
    {
        internal MultiIndexedIndicesEnumerator _inner;

        internal readonly EnginesRoot.LocatorMap _locatorMap;
        internal readonly NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private readonly NB<T1> _components;

        internal uint _joinedCurrent;

        internal JoinedIndexedIndicesEnumerator(
            in MultiIndexedIndicesEnumerator inner,
            in EnginesRoot.LocatorMap locatorMap, in NativeEGIDMapper<RowIdentityComponent> egidMapper,
            in NB<T1> components) : this()
        {
            _inner = inner;

            _locatorMap = locatorMap;
            _egidMapper = egidMapper;
            _components = components;
        }

        public bool MoveNext()
        {
            while (_inner.MoveNext())
            {
                ref var component = ref _components[_inner._current];

                // in some condition reference is not recent because user didn't call Update
                // we skip iteration in all those cases
                if (component.reference == EntityReference.Invalid)
                    continue;

                if (!_locatorMap.TryGetEGID(component.reference, out var egid))
                    continue;

                if (egid.groupID != _egidMapper.groupID)
                    continue;

                if (!_egidMapper.FindIndex(egid.entityID, out _joinedCurrent))
                    continue;

                return true;
            }

            return false;
        }

        public void Reset()
        {
            _inner.Reset();
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public (uint, uint) Current => (_inner._current, _joinedCurrent);
    }

    public readonly ref struct JoinedIndexedIndices<T1>
        where T1 : unmanaged, IForeignKeyComponent
    {
        internal readonly MultiIndexedIndices _inner;

        private readonly EnginesRoot.LocatorMap _locatorMap;
        private readonly NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private readonly NB<T1> _components;

        public JoinedIndexedIndices(
            in MultiIndexedIndices inner,
            in EnginesRoot.LocatorMap locatorMap,
            in NativeEGIDMapper<RowIdentityComponent> egidMapper,
            in NB<T1> components)
        {
            _inner = inner;
            _locatorMap = locatorMap;
            _egidMapper = egidMapper;
            _components = components;
        }

        public JoinedIndexedIndicesEnumerator<T1> GetEnumerator() =>
            new(_inner.GetEnumerator(), _locatorMap, _egidMapper, _components);
    }
}
