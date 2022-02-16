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

        public class MerchantShard : IEntityShard
        {
            internal Table<ItemDescriptor> _items = new Table<ItemDescriptor>(100);
            public Group<ItemDescriptor> Item(int groupId) => _items.Group(groupId);
        }

        public class TestSchema : IEntitySchema
        {
            public SchemaContext Context { get; set; }

            internal static Table<CharacterDescriptor> _characters = new Table<CharacterDescriptor>(100);
            public static Group<CharacterDescriptor> Character(int groupId) => _characters.Group(groupId);

            internal static Table<EquipmentDescriptor> _equipments = new Table<EquipmentDescriptor>(30);
            public static Group<EquipmentDescriptor> Equipment(int groupId) => _equipments.Group(groupId);

            internal static Partition<MerchantShard> _merchants = new Partition<MerchantShard>(50);
            public static MerchantShard Merchant(int shardId) => _merchants.Shard(shardId);
        }

        [Fact]
        public void GroupIndexTests()
        {
            Assert.Equal(TestSchema._characters.exclusiveGroup + 0, TestSchema.Character(0));
            Assert.Equal(TestSchema._characters.exclusiveGroup + 10, TestSchema.Character(10));
            Assert.Equal(TestSchema._equipments.exclusiveGroup + 29, TestSchema.Equipment(29));

            Assert.Equal(TestSchema._merchants.shards[4]._items.exclusiveGroup + 3, TestSchema.Merchant(4).Item(3));
            Assert.Equal(TestSchema._merchants.shards[31]._items.exclusiveGroup + 10, TestSchema.Merchant(31).Item(10));
        }
    }
}
