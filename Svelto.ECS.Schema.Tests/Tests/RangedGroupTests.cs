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

        public struct MerchantShard : IEntityShard
        {
            public ShardOffset Offset { get; set; }

            internal static Table<ItemDescriptor> _items = new Table<ItemDescriptor>(100);
            public Group<ItemDescriptor> Item(int groupId) => _items.At(Offset).Group(groupId);
        }

        public class TestSchema : IEntitySchema<TestSchema>
        {
            public SchemaContext Context { get; set; }

            internal static Table<CharacterDescriptor> _characters = new Table<CharacterDescriptor>(100);
            public Group<CharacterDescriptor> Character(int groupId) => _characters.Group(groupId);

            internal static Table<EquipmentDescriptor> _equipments = new Table<EquipmentDescriptor>(30);
            public Group<EquipmentDescriptor> Equipment(int groupId) => _equipments.Group(groupId);

            internal static Partition<MerchantShard> _merchants = new Partition<MerchantShard>(50);
            public MerchantShard Merchant(int shardId) => _merchants.Shard(shardId);
        }

        [Fact]
        public void MetadataTest()
        {
            var root = IEntitySchema<TestSchema>.metadata.root;

            Assert.Equal(1, root.groupSize);
            Assert.Equal(2, root.tables.count);
            Assert.Equal(1, root.partitions.count);
            Assert.Equal(1, root.partitions[0].tables.count);

            Assert.Null(root.indexers);
            Assert.Null(root.partitions[0].indexers);
            Assert.Null(root.partitions[0].partitions);

            // index is alphabetical order of field name...
            Assert.Equal(TestSchema._characters, root.tables[0].element);
            Assert.Equal(TestSchema._equipments, root.tables[1].element);

            Assert.Equal(0, TestSchema._characters.siblingOrder);
            Assert.Equal(1, TestSchema._equipments.siblingOrder);
            Assert.Equal(0, MerchantShard._items.siblingOrder);

            Assert.Equal(100, TestSchema._characters.range);
            Assert.Equal(100, root.tables[0].groupSize);

            Assert.Equal(30, TestSchema._equipments.range);
            Assert.Equal(30, root.tables[1].groupSize);

            Assert.Equal(TestSchema._merchants, root.partitions[0].element);
            Assert.Equal(MerchantShard._items, root.partitions[0].tables[0].element);

            Assert.Equal(100, MerchantShard._items.range);
            Assert.Equal(5000, root.partitions[0].tables[0].groupSize);

            Assert.Equal(root.partitions[0], _schemaContext.Merchant(13).Offset.node);
            Assert.Equal(5, _schemaContext.Merchant(5).Offset.index);
        }

        [Fact]
        public void GroupIndexTests()
        {
            var root = IEntitySchema<TestSchema>.metadata.root;
            Assert.Equal(root.tables[0].group + 0, _schemaContext.Character(0));
            Assert.Equal(root.tables[0].group + 10, _schemaContext.Character(10));
            Assert.Equal(root.tables[1].group + 29, _schemaContext.Equipment(29));

            // nested group index = parent index * range + index
            var merchant = root.partitions[0];
            Assert.Equal(merchant.tables[0].group + 403, _schemaContext.Merchant(4).Item(3));
            Assert.Equal(merchant.tables[0].group + 3110, _schemaContext.Merchant(31).Item(10));
        }
    }
}
