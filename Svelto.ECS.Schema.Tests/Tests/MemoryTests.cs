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
        public class ItemOwner : IIndexedRow<int, ItemOwner.Unique>
        {
            public struct Unique : IUnique {}
        }

        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }
        public class ItemDescriptor : GenericEntityDescriptor<ItemOwner.Component> { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterDescriptor> Character = new Table<CharacterDescriptor>();
            public readonly Tables<ItemDescriptor> Items = new Tables<ItemDescriptor>(10);

            public readonly ItemOwner.Index ItemsByOwner = new ItemOwner.Index();
        }

        public MemoryTests() : base()
        {
            for (int i = 0; i < 100; ++i)
            {
                _schema.Character.Build(_factory, (uint)i);

                for (int j = 0; j < 1000; ++j)
                {
                    var itemBuilder = _schema.Items[j / 100].Build(_factory, (uint)((i * 1000) + j));
                    itemBuilder.Init(new ItemOwner.Component(i));
                }
            }

            _submissionScheduler.SubmitEntities();
        }

        [Fact]
        public void GroupEntitiesTest()
        {
            // warming up
            var (egid, count) = _schema.Character.Entities<EGIDComponent>(_indexedDB);
            var (indexed, count2) = _schema.Items[0].Entities<ItemOwner.Component>(_indexedDB);

            long before = GC.GetAllocatedBytesForCurrentThread();

            (egid, count) = _schema.Character.Entities<EGIDComponent>(_indexedDB);
            (indexed, count2) = _schema.Items[0].Entities<ItemOwner.Component>(_indexedDB);

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }

        [Fact]
        public void GroupsEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, count), group) in _schema.Items.Entities<ItemOwner.Component>(_indexedDB))
            {
                ++loop;

                Assert.Equal(10000, count);
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, count), group) in _schema.Items.Entities<ItemOwner.Component>(_indexedDB))
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
            foreach (var ((indexed, count), indices, group) in _schema.ItemsByOwner.Where(0).Entities<ItemOwner.Component>(_indexedDB))
            {
                ++loop;

                Assert.Equal(100, indices.Count());
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, count), indices, group) in _schema.ItemsByOwner.Where(0).Entities<ItemOwner.Component>(_indexedDB))
            {
                ++loop;
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
        }
    }
}
