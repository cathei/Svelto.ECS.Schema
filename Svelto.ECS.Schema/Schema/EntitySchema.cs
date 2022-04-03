using Svelto.ECS.Schema.Internal;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    public class EntitySchema : IEntitySchema
    {
        internal readonly FilterContextID filterContextID = EntitiesDB.SveltoFilters.GetNewContextID();
    }
}