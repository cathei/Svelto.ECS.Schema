using System;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class EntityRemovalTests : SchemaTestsBase<EntityRemovalTests.TestSchema>
    {
        public struct ThiefCopmonent : IEntityComponent
        {
            public uint proof;
        }

        public struct PartitionComponent : IKeyComponent<int>
        {
            public int key { get; set; }
        }

        public struct PoliceComponent : IForeignKeyComponent
        {
            public EntityReference reference { get; set; }
            public uint proof;
        }

        public struct TestIndexComponent : IKeyComponent<int>
        {
            public int key { get; set; }
        }

        public struct ThiefSet : IResultSet<ThiefCopmonent>
        {
            public NB<ThiefCopmonent> thief;

            public void Init(in EntityCollection<ThiefCopmonent> buffers)
            {
                (thief, _) = buffers;
            }
        }

        public struct PoliceSet : IResultSet<PoliceComponent, TestIndexComponent>
        {
            public NB<PoliceComponent> police;
            public NB<TestIndexComponent> index;

            public void Init(in EntityCollection<PoliceComponent, TestIndexComponent> buffers)
            {
                (police, index, _) = buffers;
            }
        }

        public sealed class ThiefRow :
            DescriptorRow<ThiefRow>, IQueryableRow<ThiefSet>,
            IPrimaryKeyRow<PartitionComponent>, IReferenceableRow<PoliceComponent>
        { }

        public sealed class PoliceRow :
            DescriptorRow<PoliceRow>, IQueryableRow<PoliceSet>, IForeignKeyRow<PoliceComponent>,
            IIndexableRow<TestIndexComponent>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<ThiefRow> Thief = new();
            public readonly Table<PoliceRow> Police = new();

            public readonly ForeignKey<PoliceComponent, ThiefRow> Target = new();
            public readonly PrimaryKey<PartitionComponent> Partition = new();

            public readonly Index<TestIndexComponent> TestIndex = new();

            public TestSchema()
            {
                Thief.AddPrimaryKeys(Partition);
                Partition.SetPossibleKeys(Enumerable.Range(0, 10).ToArray());
            }
        }

        [Fact]
        public void TestEntityReferenceRemoval()
        {
            var builder = _factory.Build(_schema.Thief, 0);
            builder.Init(new PartitionComponent { key = 4 });

            var theifRef = builder.reference;

            builder = _factory.Build(_schema.Police, 0);
            builder.Init(new PoliceComponent { reference = theifRef });

            var policeRef = builder.reference;

            _submissionScheduler.SubmitEntities();

            int loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().FromAll<PoliceRow>()
                                            .Join<ThiefSet>().On(_schema.Target))
            {
                foreach (var (ia, ib) in result.indices)
                {
                    ++loop;
                }
            }

            Assert.Equal(1, loop);

            foreach (var result in _indexedDB.From(_schema.Thief).Where(EntityID.Is(0)))
            {
                foreach (var i in result.indices)
                    _indexedDB.Remove(result.egid[i]);
            }

            _submissionScheduler.SubmitEntities();

            loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().FromAll<PoliceRow>()
                                            .Join<ThiefSet>().On(_schema.Target))
            {
                foreach (var (ia, ib) in result.indices)
                {
                    ++loop;
                }
            }

            Assert.Equal(0, loop);
        }

        [Fact]
        public void TestFilterRemoval()
        {
            for (uint i = 0; i < 100; ++i)
            {
                var builder = _factory.Build(_schema.Police, i);
                builder.Init(new TestIndexComponent { key = (int)(i % 2) });
            }

            _submissionScheduler.SubmitEntities();

            int loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().From(_schema.Police)
                                            .Where(_schema.TestIndex.Is(1)))
            {
                foreach (var i in result.indices)
                {
                    _indexedDB.Update(ref result.set.index[i], result.egid[i], 0);
                    ++loop;
                }
            }

            Assert.Equal(50, loop);

            loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().From(_schema.Police)
                                            .Where(_schema.TestIndex.Is(1)))
            {
                foreach (var i in result.indices)
                    ++loop;
            }

            Assert.Equal(0, loop);

            loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().From(_schema.Police)
                                            .Where(_schema.TestIndex.Is(0)))
            {
                foreach (var i in result.indices)
                    ++loop;
            }

            Assert.Equal(100, loop);
        }
    }
}
