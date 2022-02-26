namespace Svelto.ECS.Schema
{
    internal class EntitySchemaLock
    {
        // we have global lock to prevent metadata generation having concurrency problem
        public static EntitySchemaLock Lock = new EntitySchemaLock();
    }

    internal static class EntitySchemaHolder<T>
        where T : class, IEntitySchema, new()
    {
        public static T Schema { get; private set; }
        public static SchemaMetadata Metadata { get; private set; }

        public static void Create()
        {
            lock (EntitySchemaLock.Lock)
            {
                if (Schema != null)
                    return;

                Schema = new T();
                Metadata = new SchemaMetadata(Schema);
            }
        }
    }

    public static class EntitySchemaExtensions
    {
        public static T GenerateSchema<T>(this EnginesRoot enginesRoot, IndexesDB indexesDB)
            where T : class, IEntitySchema, new()
        {
            // Root schema - metadata pair will not directly created
            EntitySchemaHolder<T>.Create();

            var schema = EntitySchemaHolder<T>.Schema;
            var metadata = EntitySchemaHolder<T>.Metadata;

            indexesDB.RegisterSchema(metadata);

            var indexers = metadata.indexersToGenerateEngine;

            foreach (var keyType in indexers.keys)
            {
                if (indexesDB.createdEngines.Contains(keyType))
                    continue;

                indexesDB.createdEngines.Add(keyType);
                indexers[keyType].AddEngines(enginesRoot, indexesDB);
            }

            return schema;
        }

        public static IndexesDB GenerateIndexesDB(this EnginesRoot enginesRoot)
        {
            var indexesDB = new IndexesDB();

            // SchemaContextEngine injects entitiesDB to IndexesDB
            enginesRoot.AddEngine(new IndexesDBEngine(indexesDB));

            return indexesDB;
        }

        public static void Remove<T>(this T schema, IEntityFunctions functions, EGID egid)
            where T : class, IEntitySchema, new()
        {
            var metadata = EntitySchemaHolder<T>.Metadata;

            if (metadata == null)
                throw new ECSException($"Schema {typeof(T).Name} is not root schema!");

            if (!metadata.groupToTable.TryGetValue(egid.groupID, out var tableNode))
                throw new ECSException("Group ID is not found on this schema!");

            tableNode.table.Remove(functions, egid.entityID);
        }
    }
}