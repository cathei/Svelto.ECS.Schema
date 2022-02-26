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
        where T : class, IEntitySchema, new()
    {
        protected SimpleEntitiesSubmissionScheduler _submissionScheduler;
        protected EnginesRoot _enginesRoot;

        protected IEntityFactory _factory;
        protected IEntityFunctions _functions;
        protected EntitiesDB _entitiesDB;

        protected IndexesDB _indexesDB;
        protected T _schema;

        public SchemaTestsBase()
        {
            _submissionScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_submissionScheduler);

            _factory = _enginesRoot.GenerateEntityFactory();
            _functions = _enginesRoot.GenerateEntityFunctions();
            _entitiesDB = ((IUnitTestingInterface)_enginesRoot).entitiesForTesting;

            _indexesDB = _enginesRoot.GenerateIndexesDB();
            _schema = _enginesRoot.AddSchema<T>(_indexesDB);
        }

        public void Dispose()
        {
            _enginesRoot.Dispose();
        }
    }
}
