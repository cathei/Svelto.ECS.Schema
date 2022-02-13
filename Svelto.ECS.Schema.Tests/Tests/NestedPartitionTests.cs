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

        public struct StateShard : IEntityShard
        {
            public ShardOffset Offset { get; set; }

            internal static Table<DoofusEntityDescriptor> _doofus = new Table<DoofusEntityDescriptor>();
            public Group<DoofusEntityDescriptor> Doofus => _doofus.At(Offset).Group();

            internal static Table<FoodEntityDescriptor> _food = new Table<FoodEntityDescriptor>((int)FoodType.MAX);
            public Group<FoodEntityDescriptor> Food(FoodType foodType) => _food.At(Offset).Group((int)foodType);
        }

        public struct TeamShard : IEntityShard
        {
            public ShardOffset Offset { get; set; }

            internal static Partition<StateShard> _state = new Partition<StateShard>((int)StateType.MAX);
            public StateShard State(StateType stateType) => _state.At(Offset).Shard((int)stateType);
        }

        public class TestSchema : IEntitySchema<TestSchema>
        {
            public SchemaContext Context { get; set; }

            internal static Partition<TeamShard> _team = new Partition<TeamShard>((int)TeamColor.MAX);
            public TeamShard Team(TeamColor color) => _team.Shard((int)color);

            internal static Partition<StateShard> _dead = new Partition<StateShard>();
            public StateShard Dead => _dead.Shard();

            public Groups<DoofusEntityDescriptor> EatingDoofuses => _team.Shards().Each(x => x.State(StateType.Eating).Doofus);
        }

        [Fact]
        public void MetadataTest()
        {
            var root = IEntitySchema<TestSchema>.metadata.root;

            Assert.Equal(2, root.partitions.count);

            Assert.Equal(1, root.partitions[0].groupSize);
            Assert.Equal(2, root.partitions[0].tables[1].groupSize);

            Assert.Equal(4, root.partitions[1].groupSize);
            Assert.Equal(1, root.partitions[1].partitions.count);
            Assert.Equal(8, root.partitions[1].partitions[0].groupSize);
            Assert.Equal(8, root.partitions[1].partitions[0].tables[0].groupSize);
            Assert.Equal(16, root.partitions[1].partitions[0].tables[1].groupSize);
            Assert.Null(root.partitions[1].partitions[0].partitions);

            Assert.Equal(TestSchema._dead, root.partitions[0].element);
            Assert.Equal(TestSchema._team, root.partitions[1].element);

            Assert.Equal(StateShard._doofus, root.partitions[0].tables[0].element);
            Assert.Equal(StateShard._doofus, root.partitions[1].partitions[0].tables[0].element);
        }

        [Fact]
        public void GroupIndexTests()
        {
            var root = IEntitySchema<TestSchema>.metadata.root;

            Assert.Equal(root.partitions[0].tables[0].group + 0, _schema.Dead.Doofus);
            Assert.Equal(root.partitions[0].tables[1].group + 0, _schema.Dead.Food(FoodType.Rotten));
            Assert.Equal(root.partitions[0].tables[1].group + 1, _schema.Dead.Food(FoodType.Good));

            Assert.Equal(root.partitions[1].partitions[0].tables[1].group + 0, _schema.Team(TeamColor.Red).State(StateType.Eating).Food(FoodType.Rotten));
            Assert.Equal(root.partitions[1].partitions[0].tables[1].group + 11, _schema.Team(TeamColor.Yellow).State(StateType.NonEating).Food(FoodType.Good));
        }
    }
}
