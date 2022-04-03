using System;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    // we have to put test collection to prevent tests running in pararell.
    // because Svelto EntityComponentIDMap.Register is not threadsafe.
    [Collection("Schema Test Collection")]
    public class SchemaTestsBase<T> : IDisposable
        where T : class, EntitySchema, new()
    {
        protected SimpleEntitiesSubmissionScheduler _submissionScheduler;
        protected EnginesRoot _enginesRoot;

        protected IEntityFactory _factory;
        protected IEntityFunctions _functions;

        protected IndexedDB _indexedDB;
        protected T _schema;

        public SchemaTestsBase()
        {
            _submissionScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_submissionScheduler);

            _factory = _enginesRoot.GenerateEntityFactory();
            _functions = _enginesRoot.GenerateEntityFunctions();

            _indexedDB = _enginesRoot.GenerateIndexedDB();
            _schema = _enginesRoot.AddSchema<T>(_indexedDB);
        }

        public void Dispose()
        {
            _enginesRoot.Dispose();
        }
    }
}
