using System.Collections.Generic;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class ReactiveEngineTests : SchemaTestsBase<ReactiveEngineTests.TestSchema>
    {
        public struct HealthComponent : IEntityComponent, INeedEGID
        {
            public EGID ID { get; set; }
        }

        public struct DamageComponent : IEntityComponent { }

        public interface IHealthRow : IReactiveRow<HealthComponent>, ISelectorRow<HealthComponent> { }
        public interface IDamageRow : IReactiveRow<DamageComponent> { }

        public class HealthReactiveEngine : ReactToRowEngine<IHealthRow, HealthComponent>
        {
            internal int added;
            internal int moved;
            internal int removed;

            public HealthReactiveEngine(IndexedDB indexedDB) : base(indexedDB) { }

            public override void Add(ref HealthComponent component, IEntityTable<IHealthRow> table, uint entityID)
            {
                added++;
            }

            public override void MovedTo(ref HealthComponent component, IEntityTable<IHealthRow> previousTable, IEntityTable<IHealthRow> table, uint entityID)
            {
                moved++;
            }

            public override void Remove(ref HealthComponent component, IEntityTable<IHealthRow> table, uint entityID)
            {
                removed++;
            }
        }

        public class RowWithHealth : DescriptorRow<RowWithHealth>, IHealthRow
        { }

        public class RowWithHealth2 : DescriptorRow<RowWithHealth2>, IHealthRow, IDamageRow
        { }

        public class RowWithoutHealth : DescriptorRow<RowWithoutHealth>, IDamageRow
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<RowWithHealth> Table1 = new Table<RowWithHealth>();
            public readonly Tables<RowWithHealth2> Table2 = new Tables<RowWithHealth2>(5);
            public readonly Table<RowWithoutHealth> Table3 = new Table<RowWithoutHealth>();
        }

        [Fact]
        public void ReactiveEngineTest()
        {
            var engine = new HealthReactiveEngine(_indexedDB);

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Table1, i);
            }

            for (uint i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Table2[(int)(i % _schema.Table2.Range)], i);
            }

            for (uint i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Table3, i);
            }

            _submissionScheduler.SubmitEntities();

            Assert.Equal(200, engine.added);
            Assert.Equal(0, engine.moved);
            Assert.Equal(0, engine.removed);

            var (health, count) = _indexedDB.Select<IHealthRow>().From(_schema.Table2[0]).Entities();

            for (int i = 0; i < count / 2; ++i)
                _functions.Move(_schema.Table2[0], health[i].ID.entityID).To(_schema.Table2[1]);

            _functions.MoveAll(_schema.Table2[3]).To(_schema.Table2[4]);

            _functions.RemoveAll(_schema.Table1);
            _functions.RemoveAll(_schema.Table3);

            _submissionScheduler.SubmitEntities();

            Assert.Equal(30, engine.moved);
            Assert.Equal(100, engine.removed);
        }
    }
}
