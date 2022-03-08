using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class RangedTableTests : SchemaTestsBase<RangedTableTests.TestSchema>
    {
        public class CharacterRow : DescriptorRow<CharacterRow> {}
        public class EquipmentRow : DescriptorRow<EquipmentRow> {}
        public class ItemRow : DescriptorRow<ItemRow> {}

        public class MerchantSchema : IEntitySchema
        {
            public readonly Tables<ItemRow> Items = new Tables<ItemRow>(100);
        }

        public class TestSchema : IEntitySchema
        {
            public readonly Tables<CharacterRow> Characters = new Tables<CharacterRow>(100);
            public readonly Tables<CharacterRow> Equipments = new Tables<CharacterRow>(30);

            public readonly Ranged<MerchantSchema> Merchants = new Ranged<MerchantSchema>(50);
        }

        [Fact]
        public void GroupNameTest()
        {
            var schemaName = typeof(TestSchema).FullName;

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Characters.0"), _schema.Characters[0].ExclusiveGroup);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Characters.10"), _schema.Characters[10].ExclusiveGroup);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Equipments.29"), _schema.Equipments[29].ExclusiveGroup);

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Merchants.4.Items.3"), _schema.Merchants[4].Items[3].ExclusiveGroup);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Merchants.31.Items.10"), _schema.Merchants[31].Items[10].ExclusiveGroup);
        }
    }
}
