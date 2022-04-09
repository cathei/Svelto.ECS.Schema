using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{
    public ref struct JoinedIndexedIndicesEnumerator<T1, T2>
        where T1 : unmanaged, IForeignKeyComponent
        where T2 : unmanaged, IForeignKeyComponent
    {
        private JoinedIndexedIndicesEnumerator<T1> _inner;
        private readonly NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private readonly NB<T2> _components;

        private uint _joinedCurrent;

        internal JoinedIndexedIndicesEnumerator(
            in JoinedIndexedIndicesEnumerator<T1> inner,
            in NativeEGIDMapper<RowIdentityComponent> egidMapper,
            in NB<T2> components) : this()
        {
            _inner = inner;
            _egidMapper = egidMapper;
            _components = components;
        }

        public bool MoveNext()
        {
            while (_inner.MoveNext())
            {
                ref var component = ref _components[_inner._inner._current];

                // in some condition reference is not recent because user didn't call Update
                // we skip iteration in all those cases
                if (component.reference == EntityReference.Invalid)
                    continue;

                if (!_inner._locatorMap.TryGetEGID(component.reference, out var egid))
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

        public (uint, uint, uint) Current => (_inner._inner._current, _inner._joinedCurrent, _joinedCurrent);
    }

    public readonly ref struct JoinedIndexedIndices<T1, T2>
        where T1 : unmanaged, IForeignKeyComponent
        where T2 : unmanaged, IForeignKeyComponent
    {
        internal readonly JoinedIndexedIndices<T1> _inner;
        private readonly NativeEGIDMapper<RowIdentityComponent> _egidMapper;
        private readonly NB<T2> _components;

        public JoinedIndexedIndices(
            in JoinedIndexedIndices<T1> inner,
            in NativeEGIDMapper<RowIdentityComponent> egidMapper,
            in NB<T2> components)
        {
            _inner = inner;
            _egidMapper = egidMapper;
            _components = components;
        }

        public JoinedIndexedIndicesEnumerator<T1, T2> GetEnumerator()
            => new(_inner.GetEnumerator(), _egidMapper, _components);
    }
}
