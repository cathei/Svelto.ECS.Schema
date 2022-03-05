using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Internal
{
    public interface IIndexedData { }

    public interface IIndexedComponent : IEntityComponent, INeedEGID { }
}

namespace Svelto.ECS.Schema
{
    public interface IIndexedData<TKey> : IIndexedData
    {
        TKey Key { get; set; }
    }

    public struct Indexed<TData> : IIndexedComponent
        where TData : unmanaged, IIndexedData
    {
        public EGID ID { get; set; }

        internal TData _data;

        public TData Data => _data;

        // should be only called for initialization
        public Indexed(in TData data) : this()
        {
            _data = data;
        }

    }
}
