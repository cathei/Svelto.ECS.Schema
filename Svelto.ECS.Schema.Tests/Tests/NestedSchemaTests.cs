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
        public enum TeamColor { Red, Blue, Yellow, Green, MAX }
        public enum FoodType { Rotten, Good, MAX }
        public enum StateType { Eating, NonEating, MAX }

        public struct TeamComponent : IPrimaryKeyComponent<EnumKey<TeamColor>>
        {
            public EnumKey<TeamColor> key { get; set; }
        }

        public struct FoodComponent : IPrimaryKeyComponent<EnumKey<FoodType>>
        {
            public EnumKey<FoodType> key { get; set; }
        }

        public struct StateComponent : IPrimaryKeyComponent<EnumKey<StateType>>
        {
            public EnumKey<StateType> key { get; set; }
        }

        public class DoofusRow : DescriptorRow<DoofusRow>,
            IPrimaryKeyRow<TeamComponent>, IPrimaryKeyRow<StateComponent>
        { }

        public class FoodRow : DescriptorRow<FoodRow>,
            IPrimaryKeyRow<TeamComponent>, IPrimaryKeyRow<FoodComponent>, IPrimaryKeyRow<StateComponent>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<DoofusRow> Doofus = new();
            public readonly Table<FoodRow> Food = new();

            public readonly PrimaryKey<TeamComponent> TeamPK = new();
            public readonly PrimaryKey<FoodComponent> FoodPK = new();
            public readonly PrimaryKey<StateComponent> StatePK = new();

            public TestSchema()
            {
                Doofus.AddPrimaryKey(TeamPK);
                Doofus.AddPrimaryKey(StatePK);

                Food.AddPrimaryKey(TeamPK);
                Food.AddPrimaryKey(StatePK);
                Food.AddPrimaryKey(FoodPK);

                TeamPK.SetPossibleKeys(new TeamColor[] {
                    TeamColor.Red, TeamColor.Blue, TeamColor.Yellow, TeamColor.Green
                });

                FoodPK.SetPossibleKeys(new FoodType[] {
                    FoodType.Good, FoodType.Rotten
                });

                StatePK.SetPossibleKeys(new StateType[] {
                    StateType.Eating, StateType.NonEating
                });
            }
        }

        [Fact]
        public void MetadataTest()
        {
            var metadata = EntitySchemaTemplate<TestSchema>.Metadata;

            Assert.NotNull(metadata);

            Assert.Equal(17 + 9, metadata.groupToTable.count);

            Assert.Equal(9, _schema.Doofus.GroupRange);
            Assert.Equal(17, _schema.Food.GroupRange);

            Assert.Equal(metadata.groupToTable[_schema.Doofus.Group],
                metadata.groupToTable[_schema.Doofus.Group + 1]);

            Assert.NotEqual(metadata.groupToTable[_schema.Doofus.Group],
                metadata.groupToTable[_schema.Food.Group]);

            Assert.Equal(0, metadata.indexers.count);
            Assert.Equal(0, metadata.indexersToGenerateEngine.count);
        }

        [Fact]
        public void GroupNameTest()
        {
            var schemaName = typeof(TestSchema).FullName;

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Doofus-(1/9)"), _schema.Doofus.Group);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Doofus-(2/9)"), _schema.Doofus.Group + 1);

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Food-(1/17)"), _schema.Food.Group);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Food-(2/17)"), _schema.Food.Group + 1);
        }
    }
}
