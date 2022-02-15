using System;
using Svelto.ECS.Schedulers;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class SchemaTestsBase<T> : IDisposable
        where T : class, IEntitySchema<T>, new()
    {
        protected SimpleEntitiesSubmissionScheduler _submissionScheduler;
        protected EnginesRoot _enginesRoot;

        protected IEntityFactory _factory;
        protected IEntityFunctions _functions;
        protected EntitiesDB _entitiesDB;

        protected T _schema;

        public SchemaTestsBase()
        {
            _submissionScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_submissionScheduler);

            _factory = _enginesRoot.GenerateEntityFactory();
            _functions = _enginesRoot.GenerateEntityFunctions();
            _entitiesDB = ((IUnitTestingInterface)_enginesRoot).entitiesForTesting;

            _schema = _enginesRoot.AddSchema<T>();
        }

        public void Dispose()
        {
            _enginesRoot.Dispose();
        }
    }
}
