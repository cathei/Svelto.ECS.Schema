using System;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class EntityReferenceTests : SchemaTestsBase<EntityReferenceTests.TestSchema>
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

        public struct ThiefSet : IResultSet<ThiefCopmonent>
        {
            public NB<ThiefCopmonent> thief;

            public int count { get; set; }

            public void Init(in EntityCollection<ThiefCopmonent> buffers)
            {
                (thief, count) = buffers;
            }
        }

        public struct PoliceSet : IResultSet<PoliceComponent>
        {
            public NB<PoliceComponent> police;

            public int count { get; set; }

            public void Init(in EntityCollection<PoliceComponent> buffers)
            {
                (police, count) = buffers;
            }
        }

        public sealed class ThiefRow :
            DescriptorRow<ThiefRow>, IQueryableRow<ThiefSet>,
            IPrimaryKeyRow<PartitionComponent>, IReferenceableRow<PoliceComponent>
        { }

        public sealed class PoliceRow :
            DescriptorRow<PoliceRow>, IQueryableRow<PoliceSet>, IForeignKeyRow<PoliceComponent>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<ThiefRow> Thief = new();
            public readonly Table<PoliceRow> Police = new();

            public readonly ForeignKey<PoliceComponent, ThiefRow> ThiefFK = new();
            public readonly PrimaryKey<PartitionComponent> Partition = new();

            public TestSchema()
            {
                Thief.AddPrimaryKeys(Partition);
                Partition.SetPossibleKeys(Enumerable.Range(0, 10).ToArray());
            }
        }

        [Fact]
        public void ReferenceTest()
        {
            for (uint i = 0; i < 1000; ++i)
            {
                var builder = _factory.Build(_schema.Thief, i);
                builder.Init(new ThiefCopmonent { proof = i * 5 });
                builder.Init(new PartitionComponent { key = (int)(i / 100) });
            }

            _submissionScheduler.SubmitEntities();

            uint policeCount = 0;

            foreach (var result in _indexedDB.Select<ThiefSet>().FromAll())
            {
                foreach (var i in result.indices)
                {
                    var builder = _factory.Build(_schema.Police, policeCount++);

                    builder.Init(new PoliceComponent
                    {
                        reference = _indexedDB.GetEntityReference(result.egid[i]),
                        proof = result.set.thief[i].proof
                    });
                }
            }

            Assert.Equal(1000u, policeCount);

            _indexedDB.Step();
            _submissionScheduler.SubmitEntities();

            int group = 0, loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>()
                                        .From(_schema.Police)
                                        .Join<ThiefSet>().On(_schema.ThiefFK))
            {
                group++;

                foreach (var (ia, ib) in result.indices)
                {
                    loop++;

                    ref var police = ref result.setA.police[ia];
                    ref var theif = ref result.setB.thief[ib];

                    Assert.Equal(police.proof, theif.proof);
                }
            }

            Assert.Equal(10, group);
            Assert.Equal(1000, loop);
        }

        [Fact]
        public void JoinWithEntityIDsTest()
        {
            var theif1Builder = _factory.Build(_schema.Thief, 0);
            theif1Builder.Init(new ThiefCopmonent { proof = 0 });
            theif1Builder.Init(new PartitionComponent { key = 0 });

            var theif2Builder = _factory.Build(_schema.Thief, 1);
            theif2Builder.Init(new ThiefCopmonent { proof = 1 });
            theif2Builder.Init(new PartitionComponent { key = 1 });

            for (uint i = 0; i < 100; ++i)
            {
                var policeBuilder = _factory.Build(_schema.Police, i);
                policeBuilder.Init(new PoliceComponent
                {
                    reference = i % 2 == 0 ?
                        theif1Builder.reference :
                        theif2Builder.reference,
                    proof = i % 2
                });
            }

            _submissionScheduler.SubmitEntities();

            _indexedDB.Step();

            var entityIDs = new uint[]
            {
                1, 2, 3, 5
            };

            int group = 0, loop = 0;

            foreach (var result in _indexedDB.Select<PoliceSet>()
                                        .FromAll<PoliceRow>()
                                        .Where(EntityID.Is(entityIDs))
                                        .Join<ThiefSet>().On(_schema.ThiefFK))
            {
                group++;

                foreach (var (ia, ib) in result.indices)
                {
                    Assert.Contains(result.egidA[ia].entityID, entityIDs);
                    Assert.Equal(result.setA.police[ia].proof, result.setB.thief[ib].proof);

                    loop++;
                }
            }

            Assert.Equal(2, group);
            Assert.Equal(entityIDs.Length, loop);
        }

        [Fact]
        public void MemoryTest()
        {
            for (uint i = 0; i < 100; ++i)
            {
                var theifBuilder = _factory.Build(_schema.Thief, i);
                theifBuilder.Init(new ThiefCopmonent { proof = i * 5 });
                theifBuilder.Init(new PartitionComponent { key = (int)(i / 10) });

                var policeBuilder = _factory.Build(_schema.Police, i);
                policeBuilder.Init(new PoliceComponent
                {
                    reference = theifBuilder.reference,
                    proof = i * 5
                });
            }

            _submissionScheduler.SubmitEntities();

            int loop = 0;

            // warmup
            for (int count = 0; count < 2; ++count)
            {
                foreach (var result in _indexedDB.Select<PoliceSet>()
                                            .From(_schema.Police)
                                            .Join<ThiefSet>().On(_schema.ThiefFK))
                {
                    foreach (var (i, j) in result.indices)
                        loop++;
                }
            }

            Assert.True(loop > 0);

            long before = GC.GetAllocatedBytesForCurrentThread();

            loop = 0;

            for (int count = 0; count < 100; ++count)
            {
                foreach (var result in _indexedDB.Select<PoliceSet>()
                                            .From(_schema.Police)
                                            .Join<ThiefSet>().On(_schema.ThiefFK))
                {
                    foreach (var (i, j) in result.indices)
                        loop++;
                }
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
        }
    }
}
