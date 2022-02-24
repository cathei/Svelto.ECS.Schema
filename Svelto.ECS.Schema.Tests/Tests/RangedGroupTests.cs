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
            internal Table<ItemDescriptor> _items = new Table<ItemDescriptor>(100);
            public Group<ItemDescriptor> Item(int groupId) => _items.Group(groupId);
        }

        public class TestSchema : IEntitySchema
        {
            internal Table<CharacterDescriptor> _characters = new Table<CharacterDescriptor>(100);
            public Group<CharacterDescriptor> Character(int groupId) => _characters.Group(groupId);

            internal Table<EquipmentDescriptor> _equipments = new Table<EquipmentDescriptor>(30);
            public Group<EquipmentDescriptor> Equipment(int groupId) => _equipments.Group(groupId);

            internal Ranged<MerchantSchema> _merchants = new Ranged<MerchantSchema>(50);
            public MerchantSchema Merchant(int merchantId) => _merchants.Schema(merchantId);
        }

        [Fact]
        public void GroupIndexTests()
        {
            Assert.Equal(_schema._characters.ExclusiveGroup + 0, _schema.Character(0));
            Assert.Equal(_schema._characters.ExclusiveGroup + 10, _schema.Character(10));
            Assert.Equal(_schema._equipments.ExclusiveGroup + 29, _schema.Equipment(29));

            Assert.Equal(_schema._merchants._schemas[4]._items.ExclusiveGroup + 3, _schema.Merchant(4).Item(3));
            Assert.Equal(_schema._merchants._schemas[31]._items.ExclusiveGroup + 10, _schema.Merchant(31).Item(10));
        }
    }
}
