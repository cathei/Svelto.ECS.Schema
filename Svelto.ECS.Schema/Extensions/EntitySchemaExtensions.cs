namespace Svelto.ECS.Schema
{
    public static class EntitySchemaExtensions
    {
        public static SchemaContext AddSchema<T>(this EnginesRoot enginesRoot)
            where T : class, IEntitySchema
        {
            var metadata = SchemaMetadata<T>.Instance;
            var context = new SchemaContext(metadata);

            var indexers = metadata.indexersToGenerateEngine;

            for (int i = 0; i < indexers.count; ++i)
            {
                enginesRoot.AddEngine(indexers.unsafeValues[i].CreateEngine(context));
            }

            return context;
        }
    }
}