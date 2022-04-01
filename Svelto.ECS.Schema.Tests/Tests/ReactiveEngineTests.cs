using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
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

        public struct HealthSet : IResultSet<HealthComponent>
        {
            public int count { get; set; }

            public NB<HealthComponent> health;

            public void Init(in EntityCollection<HealthComponent> buffers)
            {
                (health, count) = buffers;
            }
        }

        public struct GroupComponent : IPrimaryKeyComponent<int>
        {
            public int key { get; set; }
        }

        public struct GroupSet : IResultSet<GroupComponent, EGIDComponent>
        {
            public NB<GroupComponent> group;
            public NB<EGIDComponent> egid;
            public int count;

            public void Init(in EntityCollection<GroupComponent, EGIDComponent> buffers)
            {
                (group, egid, count) = buffers;
            }
        }

        public interface IHealthRow : IReactiveRow<HealthComponent>, IQueryableRow<HealthSet> { }
        public interface IDamageRow : IReactiveRow<DamageComponent> { }

        public class HealthReactiveEngine : ReactToRowEngine<IHealthRow, HealthComponent>
        {
            internal int added;
            internal int moved;
            internal int removed;

            public HealthReactiveEngine(IndexedDB indexedDB) : base(indexedDB)
            { }

            protected override void Add(ref HealthComponent component, IEntityTable<IHealthRow> table, EGID egid)
            {
                added++;
            }

            protected override void MovedTo(ref HealthComponent component, IEntityTable<IHealthRow> previousTable, IEntityTable<IHealthRow> table, EGID egid)
            {
                moved++;
            }

            protected override void Remove(ref HealthComponent component, IEntityTable<IHealthRow> table, EGID egid)
            {
                removed++;
            }
        }

        public class RowWithHealth : DescriptorRow<RowWithHealth>, IHealthRow
        { }

        public class RowWithHealth2 : DescriptorRow<RowWithHealth2>, IHealthRow, IDamageRow,
            IPrimaryKeyRow<GroupComponent>, IQueryableRow<GroupSet>
        { }

        public class RowWithoutHealth : DescriptorRow<RowWithoutHealth>, IDamageRow
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<RowWithHealth> Table1 = new();
            public readonly Table<RowWithHealth2> Table2 = new();
            public readonly Table<RowWithoutHealth> Table3 = new();

            public readonly PrimaryKey<GroupComponent> Group = new();

            public TestSchema()
            {
                Group.SetPossibleKeys(Enumerable.Range(0, 5).ToArray());

                Table2.AddPrimaryKey(Group);
            }
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
                var builder = _factory.Build(_schema.Table2, i);
                builder.Init(new GroupComponent { key = (int)(i % _schema.Table2.GroupRange) });
            }

            for (uint i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Table3, i);
            }

            _submissionScheduler.SubmitEntities();

            Assert.Equal(200, engine.added);
            Assert.Equal(0, engine.moved);
            Assert.Equal(0, engine.removed);

            foreach (var query in _indexedDB.From(_schema.Table2))
            {
                query.Select(out GroupSet result);

                for (int i = 0; i < result.count / 2; ++i)
                    _indexedDB.Update(ref result.group[i], result.egid[i].ID, 1);
            }

            foreach (var query in _indexedDB.From(_schema.Table2).Where(_schema.Group.Is(3)))
            {
                query.Select(out GroupSet result);

                foreach (var i in query.indices)
                    _indexedDB.Update(ref result.group[i], result.egid[i].ID, 4);
            }

            _indexedDB.RemoveAll(_schema.Table1);
            _indexedDB.RemoveAll(_schema.Table3);

            _submissionScheduler.SubmitEntities();

            Assert.Equal(30, engine.moved);
            Assert.Equal(100, engine.removed);
        }
    }
}
