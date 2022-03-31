using System;
using System.Linq;
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

        public struct ItemGroupComponent : IPrimaryKeyComponent<int>
        {
            public int key { get; set; }
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
            IIndexableRow<ItemOwnerComponent>, IPrimaryKeyRow<ItemGroupComponent>,
            IQueryableRow<ItemWithOwnerSet>, IQueryableRow<EGIDSet>
        { }

        public class CharacterRow : DescriptorRow<CharacterRow>, IQueryableRow<EGIDSet>
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new();
            public readonly Table<ItemRow> Item = new();

            public readonly Index<ItemOwnerComponent> ItemOwner = new();
            public readonly PrimaryKey<ItemGroupComponent> ItemGroup = new();

            public TestSchema()
            {
                Item.AddPrimaryKey(ItemGroup);
                ItemGroup.SetPossibleKeys(Enumerable.Range(0, 10).ToArray());
            }
        }

        public MemoryTests() : base()
        {
            for (int i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Character, (uint)i);

                for (int j = 0; j < 1000; ++j)
                {
                    var itemBuilder = _factory.Build(_schema.Item, (uint)((i * 1000) + j));
                    itemBuilder.Init(new ItemOwnerComponent(i));
                    itemBuilder.Init(new ItemGroupComponent { key = j / 100 });
                }
            }

            _submissionScheduler.SubmitEntities();
        }

        [Fact]
        public void GroupEntitiesTest()
        {
            // warming up
            foreach (var query in _indexedDB.From(_schema.Character))
            {
                query.Select(out EGIDSet result);
            }

            foreach (var query in _indexedDB.From(_schema.Item))
            {
                query.Select(out ItemWithOwnerSet result);
            }

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                foreach (var query in _indexedDB.From(_schema.Character))
                {
                    query.Select(out EGIDSet result);
                }

                foreach (var query in _indexedDB.From(_schema.Item))
                {
                    query.Select(out ItemWithOwnerSet result);
                }
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }

        [Fact]
        public void GroupsEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var query in _indexedDB.From(_schema.Item))
            {
                ++loop;

                query.Select(out ItemWithOwnerSet result);

                Assert.Equal(10000, result.count);
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                foreach (var query in _indexedDB.From(_schema.Item))
                {
                    ++loop;

                    query.Select(out ItemWithOwnerSet result);
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
            foreach (var query in _indexedDB.From<ItemRow>().Where(_schema.ItemOwner.Is(0)))
            {
                ++loop;

                int indicesCount = 0;

                foreach (var i in query.indices)
                    ++indicesCount;

                Assert.Equal(100, indicesCount);
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                foreach (var query in _indexedDB.From<ItemRow>().Where(_schema.ItemOwner.Is(0)))
                {
                    ++loop;
                }
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
        }
    }
}
