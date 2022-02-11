namespace Svelto.ECS.Schema
{
    public interface IEntitySchema<T> : IEntitySchemaElement where T : class, IEntitySchema<T>, new()
    {
        internal static readonly SchemaMetadata metadata = new SchemaMetadata(typeof(T));

        public SchemaContext Context { get; set; }
    }

    public static class EntitySchemaExtensions
    {
        public static T AddSchema<T>(this EnginesRoot enginesRoot) where T : class, IEntitySchema<T>, new()
        {
            var metadata = IEntitySchema<T>.metadata;
            var context = new SchemaContext(metadata);

            var indexers = metadata.indexersToGenerateEngine;

            for (int i = 0; i < indexers.count; ++i)
            {
                enginesRoot.AddEngine(indexers.unsafeValues[i].CreateEngine(context));
            }

            return new T { Context = context };
        }
    }
}