using System.Linq;
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

        public interface IThiefRow : ISelectorRow<EGIDComponent, ThiefCopmonent>
        { }

        public sealed class ThiefRow : DescriptorRow<ThiefRow>, IThiefRow
        { }

        public sealed class PoliceRow : DescriptorRow<PoliceRow>, ISelectorRow<PoliceComponent>
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

            foreach (var ((egid, thief, count), table) in _indexedDB.Select<ThiefRow>().All().Entities())
            {
                for (uint i = 0 ; i < count; ++i)
                {
                    var builder = _factory.Build(_schema.Police, policeCount++);

                    builder.Init(new PoliceComponent
                    {
                        target = _indexedDB.GetEntityReference(table, egid[i].ID.entityID),
                        proof = thief[i].proof
                    });
                }
            }

            Assert.Equal(1000u, policeCount);

            _submissionScheduler.SubmitEntities();

            var (police, pcount) = _indexedDB.Select<PoliceRow>().From(_schema.Police).Entities();

            Assert.Equal(1000, pcount);

            for (int i = 0; i < 1000; ++i)
            {
                Assert.True(_indexedDB.TryGetEntityIndex<IThiefRow>(
                    police[i].target, out var table, out var index));

                var (egid, thief, count) = _indexedDB.Select<IThiefRow>().From(table).Entities();

                Assert.Equal(police[i].proof, thief[index].proof);
            }
        }
    }
}
