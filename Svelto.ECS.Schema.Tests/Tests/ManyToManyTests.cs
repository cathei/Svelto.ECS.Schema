using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class ManyToManyTests : SchemaTestsBase<ManyToManyTests.TestSchema>
    {
        public struct ForeignKeyA : IForeignKeyComponent
        {
            public EntityReference reference { get; set; }
        }

        public struct ForeignKeyB : IForeignKeyComponent
        {
            public EntityReference reference { get; set; }
        }

        public struct DummyComponent : IEntityComponent { }

        public struct DummyResultSet : IResultSet<DummyComponent>
        {
            public NativeEntityIDs entityIDs;

            public void Init(in EntityCollection<DummyComponent> buffers)
            {
                (_, entityIDs, _) = buffers;
            }
        }

        public class JoinRow : DescriptorRow<JoinRow>,
            IForeignKeyRow<ForeignKeyA>, IForeignKeyRow<ForeignKeyB>
        { }

        public class EntityARow : DescriptorRow<EntityARow>,
            IReferenceableRow<ForeignKeyA>, IQueryableRow<DummyResultSet>
        { }

        public class EntityBRow : DescriptorRow<EntityBRow>,
            IReferenceableRow<ForeignKeyB>, IQueryableRow<DummyResultSet>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<EntityARow> Character = new();
            public readonly Table<EntityBRow> Deck = new();
            public readonly Table<JoinRow> CharacterInDeck = new();

            public readonly ForeignKey<ForeignKeyA, EntityARow> CharacterFK = new();
            public readonly ForeignKey<ForeignKeyB, EntityBRow> DeckFK = new();
        }

        [Fact]
        public void ManyToManyTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var builder = _factory.Build(_schema.Character, i);
            }

            for (uint i = 0; i < 10; ++i)
            {
                var builder = _factory.Build(_schema.Deck, i);
            }

            _submissionScheduler.SubmitEntities();

            uint joinCount = 0;

            foreach (var resultA in _indexedDB.FromAll<EntityARow>())
            foreach (var resultB in _indexedDB.FromAll<EntityBRow>())
            {
                foreach (var i in resultA.indices)
                foreach (var j in resultB.indices)
                {
                    if ((i + j) % 2 == 0)
                        continue;

                    var builder = _factory.Build(_schema.CharacterInDeck, joinCount++);

                    builder.Init(new ForeignKeyA
                    {
                        reference = _indexedDB.GetEntityReference(resultA.egid[i])
                    });

                    builder.Init(new ForeignKeyB
                    {
                        reference = _indexedDB.GetEntityReference(resultB.egid[j])
                    });
                }
            }

            _submissionScheduler.SubmitEntities();

            uint groupCount = 0, loopCount = 0;

            foreach (var result in _indexedDB.From(_schema.CharacterInDeck)
                                        .Join<DummyResultSet>().On(_schema.CharacterFK)
                                        .Join<DummyResultSet>().On(_schema.DeckFK))
            {
                groupCount++;

                foreach (var (ia, ib, ic) in result.indices)
                {
                    loopCount++;
                    Assert.True((ib + ic) % 2 != 0);
                }
            }

            Assert.True(groupCount > 0);
            Assert.True(loopCount > 0);
        }
    }
}
