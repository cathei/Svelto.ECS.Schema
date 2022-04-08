using System;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class EntitySwapTests : SchemaTestsBase<EntitySwapTests.TestSchema>
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

        public struct ThiefSet : IResultSet<ThiefCopmonent, PartitionComponent, TestIndexComponent>
        {
            public NB<ThiefCopmonent> thief;
            public NB<PartitionComponent> partition;
            public NB<TestIndexComponent> index;

            public void Init(in EntityCollection<ThiefCopmonent, PartitionComponent, TestIndexComponent> buffers)
            {
                (thief, partition, index, _) = buffers;
            }
        }

        public struct PoliceSet : IResultSet<PoliceComponent>
        {
            public NB<PoliceComponent> police;

            public void Init(in EntityCollection<PoliceComponent> buffers)
            {
                (police, _) = buffers;
            }
        }

        public sealed class ThiefRow :
            DescriptorRow<ThiefRow>, IQueryableRow<ThiefSet>,
            IPrimaryKeyRow<PartitionComponent>, IReferenceableRow<PoliceComponent>,
            IIndexableRow<TestIndexComponent>
        { }

        public sealed class PoliceRow :
            DescriptorRow<PoliceRow>, IQueryableRow<PoliceSet>, IForeignKeyRow<PoliceComponent>
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
        public void TestEntityReferenceSwap()
        {
            var builder = _factory.Build(_schema.Thief, 0);
            builder.Init(new PartitionComponent { key = 4 });

            var theifRef = builder.reference;

            builder = _factory.Build(_schema.Police, 0);
            builder.Init(new PoliceComponent { reference = theifRef });

            _submissionScheduler.SubmitEntities();

            int loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().FromAll<PoliceRow>()
                                            .Join<ThiefSet>().On(_schema.Target))
            {
                foreach (var (ia, ib) in result.indices)
                {
                    ++loop;

                    _indexedDB.Update(ref result.setB.partition[ib], result.egidB[ib], 2);
                }

                Assert.Equal(_schema.Thief.Group + 4 + 1, result.groupB);
            }

            Assert.Equal(1, loop);

            _indexedDB.Step();
            _submissionScheduler.SubmitEntities();

            loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>().FromAll<PoliceRow>()
                                            .Join<ThiefSet>().On(_schema.Target))
            {
                foreach (var (ia, ib) in result.indices)
                {
                    ++loop;
                }

                Assert.Equal(_schema.Thief.Group + 2 + 1, result.groupB);
            }

            Assert.Equal(1, loop);
        }

        [Fact]
        public void TestFilterSwap()
        {
            for (uint i = 0; i < 100; ++i)
            {
                var builder = _factory.Build(_schema.Thief, i);
                builder.Init(new TestIndexComponent { key = (int)(i % 2) });
                builder.Init(new PartitionComponent { key = (int)(i % 5) });
            }

            _submissionScheduler.SubmitEntities();

            int groupCount = 0;

            foreach (var result in _indexedDB.Select<ThiefSet>().From(_schema.Thief)
                                            .Where(_schema.TestIndex.Is(1)))
            {
                int entityCount = 0;

                foreach (var i in result.indices)
                {
                    _indexedDB.Update(ref result.set.partition[i], result.egid[i], 9);
                    ++entityCount;
                }

                Assert.Equal(10, entityCount);

                ++groupCount;
            }

            Assert.Equal(5, groupCount);

            _indexedDB.Step();
            _submissionScheduler.SubmitEntities();

            groupCount = 0;

            foreach (var result in _indexedDB.Select<ThiefSet>().From(_schema.Thief)
                                            .Where(_schema.TestIndex.Is(1)))
            {
                int entityCount = 0;

                foreach (var i in result.indices)
                    ++entityCount;

                Assert.Equal(50, entityCount);

                ++groupCount;
            }

            Assert.Equal(1, groupCount);
        }
    }
}
