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

        public struct PartitionComponent : IPrimaryKeyComponent<int>
        {
            public int key { get; set; }
        }

        public struct PoliceComponent : IEntityComponent
        {
            public EntityReference target;
            public uint proof;
        }

        public struct ThiefSet : IResultSet<EGIDComponent, ThiefCopmonent>
        {
            public NB<EGIDComponent> egid;
            public NB<ThiefCopmonent> thief;

            public int count { get; set; }

            public void Init(in EntityCollection<EGIDComponent, ThiefCopmonent> buffers)
            {
                (egid, thief, count) = buffers;
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
            DescriptorRow<ThiefRow>, IQueryableRow<ThiefSet>, IPrimaryKeyRow<PartitionComponent>
        { }

        public sealed class PoliceRow :
            DescriptorRow<PoliceRow>, IQueryableRow<PoliceSet>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<ThiefRow> Thief = new();
            public readonly Table<PoliceRow> Police = new();

            public readonly PrimaryKey<PartitionComponent> Partition = new();

            public TestSchema()
            {
                Thief.AddPrimaryKey(Partition);
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
            _indexedDB.Engine.Step();

            uint policeCount = 0;

            foreach (var query in _indexedDB.From<ThiefSet>())
            {
                query.Select(out var result);

                foreach (var i in query.indices)
                {
                    var builder = _factory.Build(_schema.Police, policeCount++);

                    builder.Init(new PoliceComponent
                    {
                        target = _indexedDB.GetEntityReference(result.egid[i].ID.entityID, query.group),
                        proof = result.thief[i].proof
                    });
                }
            }

            Assert.Equal(1000u, policeCount);

            _submissionScheduler.SubmitEntities();
            _indexedDB.Engine.Step();

            foreach (var query in _indexedDB.From(_schema.Police))
            {
                query.Select(out PoliceSet result);

                Assert.Equal(1000, result.count);

                for (int i = 0; i < 1000; ++i)
                {
                    Assert.True(_indexedDB.TryGetEntityIndex<ThiefRow>(
                        result.police[i].target, out var index));

                    // TODO Foriegn Key

                    // var thiefResult = _indexedDB.Select<ThiefSet>().From(table).Entities();
                    // Assert.Equal(result.police[i].proof, thiefResult.thief[index].proof);
                }
            }
        }
    }
}
