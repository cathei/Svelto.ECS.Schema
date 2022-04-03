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
        public struct ItemOwnerComponent : IKeyComponent<int>
        {
            public EGID ID { get; set; }
            public int key { get; set; }

            public ItemOwnerComponent(int itemOwner) : this()
            {
                key = itemOwner;
            }
        }

        public struct ItemGroupComponent : IKeyComponent<int>
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

        public class TestSchema : EntitySchema
        {
            public readonly Table<CharacterRow> Character = new();
            public readonly Table<ItemRow> Item = new();

            public readonly Index<ItemOwnerComponent> ItemOwner = new();
            public readonly PrimaryKey<ItemGroupComponent> ItemGroup = new();

            public TestSchema()
            {
                Item.AddPrimaryKey(ItemGroup);
                ItemGroup.SetPossibleKeys(Enumerable.Range(0, ItemGroupCount).ToArray());
            }
        }

        // Currently some performance issue for Swap calls
        private const int CharacterCount = 10;//100;
        private const int ItemPerCharacter = 10;//1000;
        private const int ItemGroupCount = 10;

        private const int ItemPerGroup = CharacterCount * ItemPerCharacter / ItemGroupCount;

        public MemoryTests() : base()
        {
            for (int i = 0; i < CharacterCount; ++i)
            {
                _factory.Build(_schema.Character, (uint)i);

                for (int j = 0; j < ItemPerCharacter; ++j)
                {
                    var itemBuilder = _factory.Build(_schema.Item, (uint)((i * ItemPerCharacter) + j));
                    itemBuilder.Init(new ItemOwnerComponent(i));
                    itemBuilder.Init(new ItemGroupComponent { key = j / (ItemPerCharacter / ItemGroupCount) });
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

                Assert.Equal(ItemPerGroup, result.count);
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
            int indicesCount = 0;

            // warming up
            foreach (var query in _indexedDB.From<ItemRow>().Where(_schema.ItemOwner.Is(0)))
            {
                ++loop;

                foreach (var index in query.indices)
                    ++indicesCount;
            }

            Assert.True(loop > 0);
            Assert.Equal(ItemPerCharacter, indicesCount);

            loop = 0;
            indicesCount = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                foreach (var query in _indexedDB.From<ItemRow>().Where(_schema.ItemOwner.Is(0)))
                {
                    ++loop;

                    foreach (var index in query.indices)
                        ++indicesCount;
                }
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
            Assert.True(indicesCount > 0);
        }
    }
}
