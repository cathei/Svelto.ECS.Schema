using Svelto.ECS.Schema.Internal;
using Svelto.ECS.Schema.Definition;

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

    public sealed class EntityIDQueryable
    {
        public EntityIDQuery Is(uint entityID) => new(entityID);
    }
}