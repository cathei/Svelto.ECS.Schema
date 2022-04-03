using Svelto.ECS.Schema.Internal;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema
{
    internal class EntitySchemaLock
    {
        // we have global lock to prevent metadata generation having concurrency problem
        public static EntitySchemaLock Lock = new EntitySchemaLock();
    }

    internal static class EntitySchemaTemplate<T>
        where T : EntitySchema, new()
    {
        public static T Schema { get; private set; }
        public static SchemaMetadata Metadata { get; private set; }

        public static void Create()
        {
            if (Schema != null)
                return;

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
        public static T AddSchema<T>(this EnginesRoot enginesRoot, IndexedDB indexedDB)
            where T : EntitySchema, new()
        {
            // Root schema - metadata pair will not be directly created
            EntitySchemaTemplate<T>.Create();

            var schema = EntitySchemaTemplate<T>.Schema;
            var metadata = EntitySchemaTemplate<T>.Metadata;

            indexedDB.RegisterSchema(enginesRoot, metadata);

            return schema;
        }

        public static IndexedDB GenerateIndexedDB(this EnginesRoot enginesRoot)
        {
            var entityFunctions = enginesRoot.GenerateEntityFunctions();
            var indexedDB = new IndexedDB(entityFunctions);

            // SchemaContextEngine injects EntitiesDB to IndexedDB
            enginesRoot.AddEngine(indexedDB.Engine);

            return indexedDB;
        }
    }
}