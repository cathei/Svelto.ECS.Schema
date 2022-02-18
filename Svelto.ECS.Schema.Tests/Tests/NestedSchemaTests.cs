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
            internal Table<DoofusEntityDescriptor> _doofus = new Table<DoofusEntityDescriptor>();
            public Group<DoofusEntityDescriptor> Doofus => _doofus.Group();

            internal Table<FoodEntityDescriptor> _food = new Table<FoodEntityDescriptor>((int)FoodType.MAX);
            public Group<FoodEntityDescriptor> Food(FoodType foodType) => _food.Group((int)foodType);
        }

        public class TeamSchema : IEntitySchema
        {
            internal Shard<StateSchema> _state = new Shard<StateSchema>((int)StateType.MAX);
            public StateSchema State(StateType stateType) => _state.Schema((int)stateType);
        }

        public class TestSchema : IEntitySchema
        {
            internal Shard<TeamSchema> _team = new Shard<TeamSchema>((int)TeamColor.MAX);
            public TeamSchema Team(TeamColor color) => _team.Schema((int)color);

            internal Shard<StateSchema> _dead = new Shard<StateSchema>();
            public StateSchema Dead => _dead.Schema();

            public Groups<DoofusEntityDescriptor> EatingDoofuses { get; }

            public TestSchema()
            {
                EatingDoofuses = _team.Schemas().Combine(x => x.State(StateType.Eating).Doofus);
            }
        }

        [Fact]
        public void MetadataTest()
        {
            var metadata = EntitySchemaHolder<TestSchema>.Metadata;

            Assert.Equal(24 + 3, metadata.groupToParentShard.count);

            Assert.Equal(metadata.root, metadata.groupToParentShard[_schema.Dead.Doofus].parent);
            Assert.Equal(metadata.root, metadata.groupToParentShard[_schema.Team(TeamColor.Red).State(StateType.Eating).Doofus].parent.parent);

            Assert.Equal(metadata.groupToParentShard[_schema.Team(TeamColor.Red).State(StateType.Eating).Doofus],
                metadata.groupToParentShard[_schema.Team(TeamColor.Red).State(StateType.Eating).Food(FoodType.Good)]);

            Assert.NotEqual(metadata.groupToParentShard[_schema.Team(TeamColor.Red).State(StateType.Eating).Doofus],
                metadata.groupToParentShard[_schema.Team(TeamColor.Blue).State(StateType.Eating).Food(FoodType.Good)]);

            Assert.Null(metadata.root.indexers);
            Assert.Equal(0, metadata.indexersToGenerateEngine.count);
        }

        [Fact]
        public void GroupIndexTests()
        {
            Assert.Equal(_schema._dead._schemas[0]._doofus.ExclusiveGroup + 0, _schema.Dead.Doofus);
            Assert.Equal(_schema._dead._schemas[0]._food.ExclusiveGroup + 0, _schema.Dead.Food(FoodType.Rotten));
            Assert.Equal(_schema._dead._schemas[0]._food.ExclusiveGroup + 1, _schema.Dead.Food(FoodType.Good));

            Assert.Equal(_schema._team._schemas[0]._state._schemas[0]._food.ExclusiveGroup + 0, _schema.Team(TeamColor.Red).State(StateType.Eating).Food(FoodType.Rotten));
            Assert.Equal(_schema._team._schemas[2]._state._schemas[1]._food.ExclusiveGroup + 1, _schema.Team(TeamColor.Yellow).State(StateType.NonEating).Food(FoodType.Good));
        }
    }
}
