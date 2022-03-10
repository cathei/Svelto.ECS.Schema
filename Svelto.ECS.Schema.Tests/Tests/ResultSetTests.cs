using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class QueryTests : SchemaTestsBase<QueryTests.TestSchema>
    {
        public struct HealthComponent : IEntityComponent
        { }

        public struct DefenseComponent : IEntityComponent
        { }

        public struct DamageComponent : IEntityComponent
        { }

        public struct PositionComponent : IEntityComponent
        { }

        // result sets

        public struct DamagableSet : IResultSet<HealthComponent, DefenseComponent>
        {
            public NB<HealthComponent> health;
            public NB<DefenseComponent> defense;

            public int count { get; set; }

            public void Init(in EntityCollection<HealthComponent, DefenseComponent> buffers)
            {
                (health, defense, count) = buffers;
            }
        }

        public struct MovableSet : IResultSet<PositionComponent>
        {
            public NB<PositionComponent> position;
            public int count { get; set; }

            public void Init(in EntityCollection<PositionComponent> buffers)
            {
                (position, count) = buffers;
            }
        }

        public struct CharacterControllerComponent : IIndexableComponent<int>
        {
            public int key { get; set; }
            public EGID ID { get; set; }
        }

        public sealed class CharacterRow : DescriptorRow<CharacterRow>,
            IQueryableRow<DamagableSet>,
            IQueryableRow<MovableSet>,
            IIndexableRow<CharacterControllerComponent>
        { }

        public class TestSchema : IEntitySchema
        {

            Table<CharacterRow> character = new Table<CharacterRow>();

        }
    }
}
