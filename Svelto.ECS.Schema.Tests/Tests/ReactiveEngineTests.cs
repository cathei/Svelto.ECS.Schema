using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema.Tests
{
    public class ReactiveEngineTests : SchemaTestsBase<ReactiveEngineTests.TestSchema>
    {
        public struct HealthComponent : IEntityComponent { }
        public struct DamageComponent : IEntityComponent { }

        public interface IHealthRow : IReactiveRow<HealthComponent> { }
        public interface IDamageRow : IReactiveRow<DamageComponent> { }

        public class HealthReactiveEngine : ReactToRowEngine<IHealthRow, HealthComponent>
        {
            public HealthReactiveEngine(IndexedDB indexedDB) : base(indexedDB) { }
        }




        public class TestSchema : IEntitySchema
        {

        }
    }
}
