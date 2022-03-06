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

            public override void Add(ref HealthComponent component, IEntityTable<IHealthRow> table, uint entityID)
            {
            }

            public override void MovedTo(ref HealthComponent component, IEntityTable<IHealthRow> previousTable, IEntityTable<IHealthRow> table, uint entityID)
            {
            }

            public override void Remove(ref HealthComponent component, IEntityTable<IHealthRow> table, uint entityID)
            {
            }
        }

        public class TestSchema : IEntitySchema
        {

        }
    }
}
