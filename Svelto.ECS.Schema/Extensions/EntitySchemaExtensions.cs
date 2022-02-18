namespace Svelto.ECS.Schema
{
    internal static class EntitySchemaHolder<T>
        where T : class, IEntitySchema, new()
    {
        public static readonly T Schema = new T();
        public static readonly SchemaMetadata Metadata = new SchemaMetadata(Schema);
    }

    public static class EntitySchemaExtensions
    {
        public static T GenerateSchema<T>(this EnginesRoot enginesRoot, IndexesDB indexesDB)
            where T : class, IEntitySchema, new()
        {
            var schema = EntitySchemaHolder<T>.Schema;
            var metadata = EntitySchemaHolder<T>.Metadata;

            indexesDB.RegisterSchema(metadata);

            var indexers = metadata.indexersToGenerateEngine;

            foreach (var keyType in indexers.keys)
            {
                if (indexesDB.createdEngines.Contains(keyType))
                    continue;

                indexesDB.createdEngines.Add(keyType);
                enginesRoot.AddEngine(indexers[keyType].CreateEngine(indexesDB));
            }

            return schema;
        }

        public static IndexesDB GenerateIndexesDB(this EnginesRoot enginesRoot)
        {
            var indexesDB = new IndexesDB();

            // SchemaContextEngine injects entitiesDB to IndexesDB
            enginesRoot.AddEngine(new SchemaContextEngine(indexesDB));

            return indexesDB;
        }
    }
}