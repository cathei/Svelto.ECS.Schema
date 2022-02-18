using System;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    // not sure if this is correct way of checking zero-allocation
    // due to Buffer<T> boxing inside of Svelto I have to have some margin here
    // just to make sure it does not take excessive memory
    public class MemoryTests : SchemaTestsBase<MemoryTests.TestSchema>
    {
        public readonly struct ItemOwner : IEntityIndexKey<ItemOwner>
        {
            public readonly int characterId;

            public ItemOwner(int characterId)
            {
                this.characterId = characterId;
            }

            public bool Equals(ItemOwner other)
            {
                return characterId.Equals(other.characterId);
            }
        }

        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class ItemDescriptor : GenericEntityDescriptor<Indexed<ItemOwner>> { }

        public class TestSchema : IEntitySchema
        {
            internal static Table<CharacterDescriptor> _character = new Table<CharacterDescriptor>();
            public static Group<CharacterDescriptor> Character => _character.Group();

            internal static Table<ItemDescriptor> _item = new Table<ItemDescriptor>(10);
            public static Group<ItemDescriptor> Item(int groupId) => _item.Group(groupId);

            internal static Index<ItemOwner> _itemsByOwner = new Index<ItemOwner>();
            public static IndexQuery<ItemOwner> ItemsByOwner(int characterId) => _itemsByOwner.Query(new ItemOwner(characterId));

            public static Groups<ItemDescriptor> AllItems { get; } = _item.Groups();
        }

        public MemoryTests() : base()
        {
            for (int i = 0; i < 100; ++i)
            {
                TestSchema.Character.Build(_factory, (uint)i);

                for (int j = 0; j < 1000; ++j)
                {
                    var itemBuilder = TestSchema.Item(j / 100).Build(_factory, (uint)((i * 1000) + j));
                    itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
                }
            }

            _submissionScheduler.SubmitEntities();
        }

        [Fact]
        public void GroupEntitiesTest()
        {
            // warming up
            var (egid, count) = TestSchema.Character.Entities<EGIDComponent>(_entitiesDB);
            var (indexed, count2) = TestSchema.Item(0).Entities<Indexed<ItemOwner>>(_entitiesDB);

            long before = GC.GetAllocatedBytesForCurrentThread();

            (egid, count) = TestSchema.Character.Entities<EGIDComponent>(_entitiesDB);
            (indexed, count2) = TestSchema.Item(0).Entities<Indexed<ItemOwner>>(_entitiesDB);

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }

        [Fact]
        public void GroupsEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, count), group) in TestSchema.AllItems.Entities<Indexed<ItemOwner>>(_entitiesDB))
            {
                ++loop;

                Assert.Equal(10000, count);
            }

            Assert.True(loop > 0);

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, count), group) in TestSchema.AllItems.Entities<Indexed<ItemOwner>>(_entitiesDB))
            {
                // do nothing
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
       }

        [Fact]
        public void IndexEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, indices), group) in TestSchema.ItemsByOwner(0).Entities<Indexed<ItemOwner>>(_schemaContext))
            {
                ++loop;

                Assert.Equal(100, indices.Count());
            }

            Assert.True(loop > 0);

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, indices), group) in TestSchema.ItemsByOwner(0).Entities<Indexed<ItemOwner>>(_schemaContext))
            {
                // do nothing
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }
    }
}
