using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class RangedGroupTests : SchemaTestsBase<RangedGroupTests.TestSchema>
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
        public void GroupIndexTests()
        {
            // Assert.Equal(_schema._characters.ExclusiveGroup + 0, _schema.Character(0));
            // Assert.Equal(_schema._characters.ExclusiveGroup + 10, _schema.Character(10));
            // Assert.Equal(_schema._equipments.ExclusiveGroup + 29, _schema.Equipment(29));

            // Assert.Equal(_schema._merchants._schemas[4]._items.ExclusiveGroup + 3, _schema.Merchant(4).Item(3));
            // Assert.Equal(_schema._merchants._schemas[31]._items.ExclusiveGroup + 10, _schema.Merchant(31).Item(10));
        }
    }
}
