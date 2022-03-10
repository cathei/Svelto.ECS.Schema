using System;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    // not sure if this is correct way of checking zero-allocation
    // due to Buffer<T> boxing inside of Svelto I have to have some margin here
    // just to make sure it does not take excessive memory
    public class MemoryTests : SchemaTestsBase<MemoryTests.TestSchema>
    {
        // components
        public struct ItemOwnerComponent : IIndexableComponent<int>
        {
            public EGID ID { get; set; }
            public int key { get; set; }

            public ItemOwnerComponent(int itemOwner) : this()
            {
                key = itemOwner;
            }
        }

        // result sets
        public struct EGIDSet : IResultSet<EGIDComponent>
        {
            public NB<EGIDComponent> egid;

            public int count { get; set; }

            public void Init(in EntityCollection<EGIDComponent> buffers)
            {
                (egid, count) = buffers;
            }
        }

        public struct ItemWithOwnerSet : IResultSet<ItemOwnerComponent>
        {
            public NB<ItemOwnerComponent> itemOwner;

            public int count { get; set; }

            public void Init(in EntityCollection<ItemOwnerComponent> buffers)
            {
                (itemOwner, count) = buffers;
            }
        }

        public class ItemRow : DescriptorRow<ItemRow>,
            IIndexableRow<ItemOwnerComponent>, IQueryableRow<ItemWithOwnerSet>, IQueryableRow<EGIDSet>
        { }

        public class CharacterRow : DescriptorRow<CharacterRow>, IQueryableRow<EGIDSet>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new Table<CharacterRow>();
            public readonly Tables<ItemRow> Items = new Tables<ItemRow>(10);

            public readonly Index<ItemOwnerComponent> ItemOwner = new Index<ItemOwnerComponent>();
        }

        public MemoryTests() : base()
        {
            for (int i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Character, (uint)i);

                for (int j = 0; j < 1000; ++j)
                {
                    var itemBuilder = _factory.Build(_schema.Items[j / 100], (uint)((i * 1000) + j));
                    itemBuilder.Init(new ItemOwnerComponent(i));
                }
            }

            _submissionScheduler.SubmitEntities();
        }

        [Fact]
        public void GroupEntitiesTest()
        {
            // warming up
            var characterResult = _indexedDB.Select<EGIDSet>().From(_schema.Character).Entities();
            var itemResult = _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Items[0]).Entities();

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                characterResult = _indexedDB.Select<EGIDSet>().From(_schema.Character).Entities();
                itemResult = _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Items[0]).Entities();
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }

        [Fact]
        public void GroupsEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var result in _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Items))
            {
                ++loop;

                Assert.Equal(10000, result.set.count);
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                foreach (var result in _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Items))
                {
                    ++loop;
                }
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
       }

        [Fact]
        public void IndexEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var result in _indexedDB
                .Select<ItemWithOwnerSet>().FromAll<ItemRow>().Where(_schema.ItemOwner.Is(0)))
            {
                ++loop;

                Assert.Equal(100, result.indices.Count());
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                foreach (var result in _indexedDB
                    .Select<ItemWithOwnerSet>().FromAll<ItemRow>().Where(_schema.ItemOwner.Is(0)))
                {
                    ++loop;
                }
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
        }
    }
}
