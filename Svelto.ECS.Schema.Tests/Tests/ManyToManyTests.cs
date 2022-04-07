using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class ManyToManyTests : SchemaTestsBase<ManyToManyTests.TestSchema>
    {
        public struct ReferenceA : IForeignKeyComponent
        {
            public EntityReference reference { get; set; }
        }

        public struct ReferenceB : IForeignKeyComponent
        {
            public EntityReference reference { get; set; }
        }

        public struct DummyComponent : IEntityComponent { }

        public struct DummyResultSet : IResultSet<DummyComponent>
        {
            public void Init(in EntityCollection<DummyComponent> buffers) { }
        }

        public class JoinRow : DescriptorRow<JoinRow>,
            IForeignKeyRow<ReferenceA>, IForeignKeyRow<ReferenceB>
        { }

        public class EntityARow : DescriptorRow<EntityARow>,
            IReferenceableRow<ReferenceA>, IQueryableRow<DummyResultSet>
        { }

        public class EntityBRow : DescriptorRow<EntityBRow>,
            IReferenceableRow<ReferenceB>, IQueryableRow<DummyResultSet>
        { }

        public class TestSchema : EntitySchema
        {
            public readonly Table<EntityARow> EntityA = new();
            public readonly Table<EntityBRow> EntityB = new();
            public readonly Table<JoinRow> Join = new();

            public readonly ForeignKey<ReferenceA, EntityARow> EntityAFK = new();
            public readonly ForeignKey<ReferenceB, EntityBRow> EntityBFK = new();
        }

        [Fact]
        public void ManyToManyTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var builder = _factory.Build(_schema.EntityA, i);
            }

            for (uint i = 0; i < 10; ++i)
            {
                var builder = _factory.Build(_schema.EntityB, i);
            }

            _submissionScheduler.SubmitEntities();

            uint joinCount = 0;

            foreach (var resultA in _indexedDB.From<EntityARow>())
            foreach (var resultB in _indexedDB.From<EntityBRow>())
            {
                foreach (var i in resultA.indices)
                foreach (var j in resultB.indices)
                {
                    if ((i + j) % 2 == 0)
                        continue;

                    var builder = _factory.Build(_schema.Join, joinCount++);

                    builder.Init(new ReferenceA
                    {
                        reference = _indexedDB.GetEntityReference(resultA.egid[i])
                    });

                    builder.Init(new ReferenceB
                    {
                        reference = _indexedDB.GetEntityReference(resultB.egid[j])
                    });
                }
            }

            _submissionScheduler.SubmitEntities();

            uint loopCount = 0;

            foreach (var resultA in _indexedDB.From<JoinRow>()
                                        .Join<DummyResultSet>()
                                        .On(_schema.EntityAFK))
            {
                // foreach (var resultB in _indexedDB.From<JoinRow>(resultA.group)
                //                             .Join()
                //                             .On(_schema.EntityBFK))
                // {

                // }

            }
        }
    }
}
