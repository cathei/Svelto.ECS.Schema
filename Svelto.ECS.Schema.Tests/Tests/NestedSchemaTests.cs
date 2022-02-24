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
        public class DoofusEntityDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class FoodEntityDescriptor : GenericEntityDescriptor<EGIDComponent> { }

        public enum TeamColor { Red, Blue, Yellow, Green, MAX }
        public enum FoodType { Rotten, Good, MAX }
        public enum StateType { Eating, NonEating, MAX }

        public class StateSchema : IEntitySchema
        {
            public readonly Table<DoofusEntityDescriptor> Doofus = new Table<DoofusEntityDescriptor>();

            public readonly Ranged<Table<FoodEntityDescriptor>, FoodType> Food =
                new Ranged<Table<FoodEntityDescriptor>, FoodType>((int)FoodType.MAX, x => (int)x);
        }

        public class TeamSchema : IEntitySchema
        {
            public readonly Ranged<StateSchema, StateType> State =
                new Ranged<StateSchema, StateType>((int)StateType.MAX, x => (int)x);
        }

        public class TestSchema : IEntitySchema
        {
            public readonly Ranged<TeamSchema, TeamColor> Team =
                new Ranged<TeamSchema, TeamColor>((int)TeamColor.MAX, x => (int)x);

            public readonly StateSchema Dead = new StateSchema();

            public readonly Tables<DoofusEntityDescriptor> EatingDoofuses;

            public TestSchema()
            {
                EatingDoofuses = Team.Combine(x => x.State[StateType.Eating].Doofus);
            }
        }

        [Fact]
        public void MetadataTest()
        {
            var metadata = EntitySchemaHolder<TestSchema>.Metadata;

            Assert.Equal(24 + 3, metadata.groupToTable.count);

            Assert.Equal(metadata.root, metadata.groupToTable[_schema.Dead.Doofus].parent.parent);
            Assert.Equal(metadata.root, metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Doofus].parent.parent.parent);

            Assert.Equal(metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Doofus].parent,
                metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Food[FoodType.Good]].parent);

            Assert.NotEqual(metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Doofus].parent,
                metadata.groupToTable[_schema.Team[TeamColor.Blue].State[StateType.Eating].Food[FoodType.Good]].parent);

            Assert.Equal(metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Food[FoodType.Rotten]],
                metadata.groupToTable[_schema.Team[TeamColor.Red].State[StateType.Eating].Food[FoodType.Good]]);

            Assert.Null(metadata.root.indexers);
            Assert.Equal(0, metadata.indexersToGenerateEngine.count);
        }

        [Fact]
        public void GroupIndexTests()
        {
            Assert.Equal(_schema.Dead._schemas[0]._doofus.ExclusiveGroup + 0, _schema.Dead.Doofus);
            Assert.Equal(_schema.Dead._schemas[0]._food.ExclusiveGroup + 0, _schema.Dead.Food[FoodType.Rotten]);
            Assert.Equal(_schema.Dead._schemas[0]._food.ExclusiveGroup + 1, _schema.Dead.Food[FoodType.Good]);

            Assert.Equal(_schema._team._schemas[0]._state._schemas[0]._food.ExclusiveGroup + 0, _schema.Team[TeamColor.Red].State[StateType.Eating].Food[FoodType.Rotten]);
            Assert.Equal(_schema._team._schemas[2]._state._schemas[1]._food.ExclusiveGroup + 1, _schema.Team[TeamColor.Yellow].State[StateType.NonEating].Food[FoodType.Good]);
        }
    }
}
