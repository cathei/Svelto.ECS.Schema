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

            public static implicit operator ItemOwner(int value) => new ItemOwner(value);
        }

        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class ItemDescriptor : GenericEntityDescriptor<Indexed<ItemOwner>> { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterDescriptor> Character = new Table<CharacterDescriptor>();
            public readonly RangedTable<ItemDescriptor> Items = new RangedTable<ItemDescriptor>(10);

            public readonly Index<ItemOwner> ItemsByOwner = new Index<ItemOwner>();

            public Tables<ItemDescriptor> AllItems { get; }

            public TestSchema()
            {
                AllItems = Items;
            }
        }

        public MemoryTests() : base()
        {
            for (int i = 0; i < 100; ++i)
            {
                _schema.Character.Build(_factory, (uint)i);

                for (int j = 0; j < 1000; ++j)
                {
                    var itemBuilder = _schema.Items[j / 100].Build(_factory, (uint)((i * 1000) + j));
                    itemBuilder.Init(new Indexed<ItemOwner>(i));
                }
            }

            _submissionScheduler.SubmitEntities();
        }

        [Fact]
        public void GroupEntitiesTest()
        {
            // warming up
            var (egid, count) = _schema.Character.Entities<EGIDComponent>(_entitiesDB);
            var (indexed, count2) = _schema.Items[0].Entities<Indexed<ItemOwner>>(_entitiesDB);

            long before = GC.GetAllocatedBytesForCurrentThread();

            (egid, count) = _schema.Character.Entities<EGIDComponent>(_entitiesDB);
            (indexed, count2) = _schema.Items[0].Entities<Indexed<ItemOwner>>(_entitiesDB);

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }

        [Fact]
        public void GroupsEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, count), group) in _schema.AllItems.Entities<Indexed<ItemOwner>>(_entitiesDB))
            {
                ++loop;

                Assert.Equal(10000, count);
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, count), group) in _schema.AllItems.Entities<Indexed<ItemOwner>>(_entitiesDB))
            {
                ++loop;
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
       }

        [Fact]
        public void IndexEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, indices), group) in _schema.ItemsByOwner.Query(0).Entities<Indexed<ItemOwner>>(_indexesDB))
            {
                ++loop;

                Assert.Equal(100, indices.Count());
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, indices), group) in _schema.ItemsByOwner.Query(0).Entities<Indexed<ItemOwner>>(_indexesDB))
            {
                ++loop;
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
        }
    }
}
