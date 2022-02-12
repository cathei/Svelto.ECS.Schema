namespace Svelto.ECS.Schema.Definition
{
    public abstract class EntitySchemaElement : IEntitySchemaElement
    {
        internal SchemaMetadata metadata;
        internal int siblingOrder;
        internal int range = 1;
    }

    public abstract class EntitySchemaIndex : EntitySchemaElement
    {
        internal abstract IEngine CreateEngine(SchemaContext context);
    }
}
