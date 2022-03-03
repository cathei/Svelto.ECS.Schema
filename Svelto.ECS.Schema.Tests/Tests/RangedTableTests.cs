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
        public interface ISimpleRow : IEntityRow<EGIDComponent> {}

        public class CharacterRow : DescriptorRow<CharacterRow>, ISimpleRow {}
        public class EquipmentRow : DescriptorRow<EquipmentRow>, ISimpleRow {}
        public class ItemRow : DescriptorRow<ItemRow>, ISimpleRow {}

        public class MerchantSchema : IEntitySchema
        {
            public readonly ItemRow.Tables Items = new ItemRow.Tables(100);
        }

        public class TestSchema : IEntitySchema
        {
            public readonly CharacterRow.Tables Characters = new CharacterRow.Tables(100);
            public readonly CharacterRow.Tables Equipments = new CharacterRow.Tables(30);

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
