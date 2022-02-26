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
        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class EquipmentDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class ItemDescriptor : GenericEntityDescriptor<EGIDComponent> { }

        public class MerchantSchema : IEntitySchema
        {
            public readonly RangedTable<ItemDescriptor> Items = new RangedTable<ItemDescriptor>(100);
        }

        public class TestSchema : IEntitySchema
        {
            public readonly RangedTable<CharacterDescriptor> Characters = new RangedTable<CharacterDescriptor>(100);
            public readonly RangedTable<EquipmentDescriptor> Equipments = new RangedTable<EquipmentDescriptor>(30);

            public readonly Ranged<MerchantSchema> Merchants = new Ranged<MerchantSchema>(50);
        }

        [Fact]
        public void GroupNameTest()
        {
            var schemaName = typeof(TestSchema).FullName;

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Characters.0"), _schema.Characters[0]);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Characters.10"), _schema.Characters[10]);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Equipments.29"), _schema.Equipments[29]);

            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Merchants.4.Items.3"), _schema.Merchants[4].Items[3]);
            Assert.Equal(ExclusiveGroup.Search($"{schemaName}.Merchants.31.Items.10"), _schema.Merchants[31].Items[10]);
        }
    }
}
