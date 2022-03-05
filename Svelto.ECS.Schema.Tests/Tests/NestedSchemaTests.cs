using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class NestedSchemaTests : SchemaTestsBase<NestedSchemaTests.TestSchema>
    {
        public interface IHaveEGID : IEntityRow<EGIDComponent> { }

        public class DoofusRow : DescriptorRow<DoofusRow>, IHaveEGID { }
        public class FoodRow : DescriptorRow<FoodRow>, IHaveEGID { }

        public enum TeamColor { Red, Blue, Yellow, Green, MAX }
        public enum FoodType { Rotten, Good, MAX }
        public enum StateType { Eating, NonEating, MAX }

        public class StateSchema : IEntitySchema
        {
            public readonly Table<DoofusRow> Doofus = new Table<DoofusRow>();

            public readonly Tables<FoodRow, FoodType> Food =
                new Tables<FoodRow, FoodType>(FoodType.MAX, x => (int)x);
        }

        public class TeamSchema : IEntitySchema
        {
            public readonly Ranged<StateSchema, StateType> State =
                new Ranged<StateSchema, StateType>(StateType.MAX, x => (int)x);
        }

        public class TestSchema : IEntitySchema
        {
            public readonly Ranged<TeamSchema, TeamColor> Team =
                new Ranged<TeamSchema, TeamColor>(TeamColor.MAX, x => (int)x);

            public readonly StateSchema Dead = new StateSchema();

            public readonly CombinedTables<DoofusRow> EatingDoofuses;

            public TestSchema()
            {
                EatingDoofuses = Team.Combine(x => x.State[StateType.Eating].Doofus);

                Team.Combine(x => x.State.Combine(x => x.Food));
            }
        }

        [Fact]
        public void MetadataTest()
        {
            // child schema metadata must be null
            Assert.Null(EntitySchemaTemplate<TeamSchema>.Metadata);

            // only root metadata valid
            var metadata = EntitySchemaTemplate<TestSchema>.Metadata;

            Assert.NotNull(metadata);

            Assert.Equal(24 + 3, metadata.groupToTable.count);

            Assert.Equal(metadata.root, metadata.groupToTable[_schema.Dead.Doofus].parent.parent);
            Assert.Equal(metadata.root, metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Doofus].parent.parent.parent);

            Assert.Equal(metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Doofus].parent,
                metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Food[FoodType.Good].ExclusiveGroup].parent);

            Assert.NotEqual(metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Doofus].parent,
                metadata.groupToTable[_schema.Team[TeamColor.Blue].State[StateType.Eating].Food[FoodType.Good].ExclusiveGroup].parent);

            Assert.Null(metadata.root.indexers);
            Assert.Equal(0, metadata.indexersToGenerateEngine.count);
        }

        [Fact]
        public void GroupNameTest()
        {
            var schemaName = typeof(TestSchema).FullName;

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Dead.Doofus"), _schema.Dead.Doofus);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Dead.Food.0"), _schema.Dead.Food[FoodType.Rotten].ExclusiveGroup);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Dead.Food.1"), _schema.Dead.Food[FoodType.Good].ExclusiveGroup);

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Team.0.State.0.Food.0"), _schema.Team[TeamColor.Red].State[StateType.Eating].Food[FoodType.Rotten].ExclusiveGroup);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Team.2.State.1.Food.1"), _schema.Team[TeamColor.Yellow].State[StateType.NonEating].Food[FoodType.Good].ExclusiveGroup);
        }
    }
}
