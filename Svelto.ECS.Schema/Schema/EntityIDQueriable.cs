using Svelto.ECS.Schema.Internal;
using Svelto.ECS.Schema.Definition;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Internal
{
    public readonly struct EntityIDQuery : IIndexQuery<IEntityRow>
    {
        internal readonly uint _entityID;

        internal EntityIDQuery(uint entityID)
        {
            _entityID = entityID;
        }

        void IIndexQuery.Apply(ResultSetQueryConfig config)
        {
            config.selectedEntityIDs.Add(_entityID);
        }
    }

    public readonly struct EntityIDMultiQuery : IIndexQuery<IEntityRow>
    {
        internal readonly FasterReadOnlyList<uint> _entityIDs;

        internal EntityIDMultiQuery(FasterReadOnlyList<uint> entityIDs)
        {
            _entityIDs = entityIDs;
        }

        void IIndexQuery.Apply(ResultSetQueryConfig config)
        {
            foreach (var entityID in _entityIDs)
                config.selectedEntityIDs.Add(entityID);
        }
    }

    public sealed class EntityIDQueryable
    {
        public EntityIDQuery Is(uint entityID) => new(entityID);
        public EntityIDMultiQuery Is(FasterReadOnlyList<uint> entityIDs) => new(entityIDs);
    }
}