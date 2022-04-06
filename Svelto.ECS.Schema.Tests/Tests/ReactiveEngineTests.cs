using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Svelto.ECS.Schema.Internal;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class ReactiveEngineTests : SchemaTestsBase<ReactiveEngineTests.TestSchema>
    {
        public struct HealthComponent : IEntityComponent { }

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

        public struct GroupComponent : IKeyComponent<int>
        {
            public int key { get; set; }
        }

        public struct GroupSet : IResultSet<GroupComponent>
        {
            public NB<GroupComponent> group;
            public int count;

            public void Init(in EntityCollection<GroupComponent> buffers)
            {
                (group, count) = buffers;
            }
        }

        public interface IHealthRow : IQueryableRow<HealthSet>, IReactiveRow<HealthComponent> { }
        public interface IDamageRow : IEntityRow<DamageComponent> { }

        public class HealthReactiveEngine :
            IReactRowAdd<IHealthRow, HealthComponent>,
            IReactRowRemove<IHealthRow, HealthComponent>,
            IReactRowSwap<IHealthRow, HealthComponent>
        {
            internal int added;
            internal int moved;
            internal int removed;

            public IndexedDB indexedDB { get; }

            public HealthReactiveEngine(IndexedDB indexedDB)
            {
                this.indexedDB = indexedDB;
            }

            public void Add(in EntityCollection<HealthComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
            {
                foreach (int i in indices)
                    added++;
            }

            public void MovedTo(in EntityCollection<HealthComponent> collection, RangedIndices indices, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
            {
                foreach (int i in indices)
                    moved++;
            }

            public void Remove(in EntityCollection<HealthComponent> collection, RangedIndices indices, ExclusiveGroupStruct group)
            {
                foreach (int i in indices)
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

        public class TestSchema : EntitySchema
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
                builder.Init(new GroupComponent { key = (int)(i % _schema.Group.PossibleKeyCount) });
            }

            for (uint i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Table3, i);
            }

            _submissionScheduler.SubmitEntities();

            Assert.Equal(200, engine.added);
            Assert.Equal(100, engine.moved);
            Assert.Equal(0, engine.removed);

            engine.added = engine.moved = engine.removed = 0;

            foreach (var result in _indexedDB.Select<GroupSet>().From(_schema.Table2).Where(_schema.Group.Is(0)))
            {
                for (int i = 0; i < result.set.count / 2; ++i)
                    _indexedDB.Update(ref result.set.group[i], result.egid[i], 1);
            }

            foreach (var result in _indexedDB.Select<GroupSet>().From(_schema.Table2).Where(_schema.Group.Is(3)))
            {
                foreach (var i in result.indices)
                    _indexedDB.Update(ref result.set.group[i], result.egid[i], 4);
            }

            _indexedDB.RemoveAll(_schema.Table1);
            _indexedDB.RemoveAll(_schema.Table3);

            _indexedDB.Engine.Step();
            _submissionScheduler.SubmitEntities();

            Assert.Equal(30, engine.moved);
            Assert.Equal(100, engine.removed);
        }
    }
}
