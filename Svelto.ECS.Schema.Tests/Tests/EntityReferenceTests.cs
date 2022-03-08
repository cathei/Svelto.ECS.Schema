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
            DescriptorRow<ThiefRow>, IQueryableRow<ThiefSet>
        { }

        public sealed class PoliceRow :
            DescriptorRow<PoliceRow>, IQueryableRow<PoliceSet>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Tables<ThiefRow> Thieves = new Tables<ThiefRow>(10);
            public readonly Table<PoliceRow> Police = new Table<PoliceRow>();
        }

        [Fact]
        public void ReferenceTest()
        {
            for (uint i = 0; i < 1000; ++i)
            {
                var builder = _factory.Build(_schema.Thieves[i / 100], i);
                builder.Init(new ThiefCopmonent { proof = i * 5 });
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
                        target = _indexedDB.GetEntityReference(result.table, result.set.egid[i].ID.entityID),
                        proof = result.set.thief[i].proof
                    });
                }
            }

            Assert.Equal(1000u, policeCount);

            _submissionScheduler.SubmitEntities();

            var policeResult = _indexedDB.Select<PoliceSet>().From(_schema.Police).Entities();

            Assert.Equal(1000, policeResult.set.count);

            for (int i = 0; i < 1000; ++i)
            {
                Assert.True(_indexedDB.TryGetEntityIndex<ThiefRow>(
                    policeResult.set.police[i].target, out var table, out var index));

                var thiefResult = _indexedDB.Select<ThiefSet>().From(table).Entities();

                Assert.Equal(policeResult.set.police[i].proof, thiefResult.set.thief[index].proof);
            }
        }
    }
}
