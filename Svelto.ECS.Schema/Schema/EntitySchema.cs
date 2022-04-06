using Svelto.ECS.Schema.Internal;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    public abstract class EntitySchema : IEntitySchema
    {
        public readonly EntityIDQueryable EntityID = new();
    }
}