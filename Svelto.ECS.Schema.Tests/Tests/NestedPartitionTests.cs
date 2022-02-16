using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class NestedPartitionTests : SchemaTestsBase<NestedPartitionTests.TestSchema>
    {
        public class DoofusEntityDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class FoodEntityDescriptor : GenericEntityDescriptor<EGIDComponent> { }

        public enum TeamColor { Red, Blue, Yellow, Green, MAX }
        public enum FoodType { Rotten, Good, MAX }
        public enum StateType { Eating, NonEating, MAX }

        public class StateShard : IEntityShard
        {
            internal Table<DoofusEntityDescriptor> _doofus = new Table<DoofusEntityDescriptor>();
            public Group<DoofusEntityDescriptor> Doofus => _doofus.Group();

            internal Table<FoodEntityDescriptor> _food = new Table<FoodEntityDescriptor>((int)FoodType.MAX);
            public Group<FoodEntityDescriptor> Food(FoodType foodType) => _food.Group((int)foodType);
        }

        public class TeamShard : IEntityShard
        {
            internal Partition<StateShard> _state = new Partition<StateShard>((int)StateType.MAX);
            public StateShard State(StateType stateType) => _state.Shard((int)stateType);
        }

        public class TestSchema : IEntitySchema
        {
            internal static Partition<TeamShard> _team = new Partition<TeamShard>((int)TeamColor.MAX);
            public static TeamShard Team(TeamColor color) => _team.Shard((int)color);

            internal static Partition<StateShard> _dead = new Partition<StateShard>();
            public static StateShard Dead => _dead.Shard();

            public static Groups<DoofusEntityDescriptor> EatingDoofuses { get; }
                = _team.Shards().Combine(x => x.State(StateType.Eating).Doofus);
        }

        [Fact]
        public void MetadataTest()
        {
            var metadata = SchemaMetadata<TestSchema>.Instance;

            Assert.Equal(24 + 3, metadata.groupToParentPartition.count);

            Assert.Equal(metadata.root, metadata.groupToParentPartition[TestSchema.Dead.Doofus].parent);
            Assert.Equal(metadata.root, metadata.groupToParentPartition[TestSchema.Team(TeamColor.Red).State(StateType.Eating).Doofus].parent.parent);

            Assert.Equal(metadata.groupToParentPartition[TestSchema.Team(TeamColor.Red).State(StateType.Eating).Doofus],
                metadata.groupToParentPartition[TestSchema.Team(TeamColor.Red).State(StateType.Eating).Food(FoodType.Good)]);

            Assert.NotEqual(metadata.groupToParentPartition[TestSchema.Team(TeamColor.Red).State(StateType.Eating).Doofus],
                metadata.groupToParentPartition[TestSchema.Team(TeamColor.Blue).State(StateType.Eating).Food(FoodType.Good)]);

            Assert.Null(metadata.root.indexers);
            Assert.Equal(0, metadata.indexersToGenerateEngine.count);
        }

        [Fact]
        public void GroupIndexTests()
        {
            Assert.Equal(TestSchema._dead.shards[0]._doofus.exclusiveGroup + 0, TestSchema.Dead.Doofus);
            Assert.Equal(TestSchema._dead.shards[0]._food.exclusiveGroup + 0, TestSchema.Dead.Food(FoodType.Rotten));
            Assert.Equal(TestSchema._dead.shards[0]._food.exclusiveGroup + 1, TestSchema.Dead.Food(FoodType.Good));

            Assert.Equal(TestSchema._team.shards[0]._state.shards[0]._food.exclusiveGroup + 0, TestSchema.Team(TeamColor.Red).State(StateType.Eating).Food(FoodType.Rotten));
            Assert.Equal(TestSchema._team.shards[2]._state.shards[1]._food.exclusiveGroup + 1, TestSchema.Team(TeamColor.Yellow).State(StateType.NonEating).Food(FoodType.Good));
        }
    }
}
