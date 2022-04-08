using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class IndexingTests : SchemaTestsBase<IndexingTests.TestSchema>
    {
        // components
        public struct ItemOwnerComponent : IKeyComponent<int>
        {
            public int key { get; set; }
            public uint entityID;

            public ItemOwnerComponent(int itemOwner, uint entityID) : this()
            {
                key = itemOwner;
                this.entityID = entityID;
            }
        }

        public struct PlayerComponent : IKeyComponent<int>
        {
            public int key { get; set; }

            public PlayerComponent(int playerId)
            {
                key = playerId;
            }
        }

        // result sets
        public struct ItemWithOwnerSet : IResultSet<ItemOwnerComponent, PlayerComponent>
        {
            public NB<ItemOwnerComponent> itemOwner;
            public NB<PlayerComponent> player;

            public int count { get; set; }

            public void Init(in EntityCollection<ItemOwnerComponent, PlayerComponent> buffers)
            {
                (itemOwner, player, count) = buffers;
            }
        }

        // rows
        public class CharacterRow : DescriptorRow<CharacterRow>,
            IPrimaryKeyRow<PlayerComponent>
        {}

        public class ItemRow : DescriptorRow<ItemRow>,
            IIndexableRow<ItemOwnerComponent>, IPrimaryKeyRow<PlayerComponent>,
            IQueryableRow<ItemWithOwnerSet>
        {}

        // schema
        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new();
            public readonly Table<ItemRow> Item = new();

            public readonly PrimaryKey<PlayerComponent> Player = new();

            public readonly Index<ItemOwnerComponent> ItemOwner = new();

            public TestSchema()
            {
                Character.AddPrimaryKeys(Player);
                Item.AddPrimaryKeys(Player);

                Player.SetPossibleKeys(Enumerable.Range(-1, 11).ToArray());
            }
        }

        private void AssertIndexer(int itemOwnerKey, int? playerId, int expectedGroupCount,
            out List<ItemOwnerComponent> entityIDs)
        {
            entityIDs = new List<ItemOwnerComponent>();

            int groupCount = 0;

            var query = _indexedDB.Select<ItemWithOwnerSet>()
                .FromAll<ItemRow>().Where(_schema.ItemOwner.Is(itemOwnerKey));

            if (playerId != null)
                query.Where(_schema.Player.Is(playerId.Value));

            foreach (var result in query)
            {
                groupCount++;

                foreach (var i in result.indices)
                    entityIDs.Add(result.set.itemOwner[i]);
            }

            Assert.Equal(expectedGroupCount, groupCount);
        }

        [Fact]
        public void IndexUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(0, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(-1));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(0, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(0, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(1));
            }

            _submissionScheduler.SubmitEntities();

            AssertIndexer(0, null, 3, out var queriedComponents);

            Assert.Equal(30, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.entityID));

            Assert.All(queriedComponents.Select(x => x.key), x => Assert.Equal(0, x));

            foreach (var result in _indexedDB.Select<ItemWithOwnerSet>()
                .From(_schema.Item).Where(_schema.Player.Is(0)))
            {
                foreach (var i in result.indices)
                    _indexedDB.Update(ref result.set.itemOwner[i], result.egid[i], 1);
            }

            AssertIndexer(0, null, 2, out queriedComponents);

            Assert.Equal(20, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.entityID));

            AssertIndexer(1, null, 1, out queriedComponents);

            Assert.Equal(10, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.entityID));
        }

        [Fact]
        public void GroupUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(0, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(-1));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(0, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(0, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(1));
            }

            _submissionScheduler.SubmitEntities();

            AssertIndexer(0, -1, 1, out var queriedComponents);

            Assert.Equal(10, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.entityID));

            foreach (var result in _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Item).Where(_schema.Player.Is(-1)))
            {
                for (int i = 0; i < result.set.count / 2; ++i)
                    _indexedDB.Update(ref result.set.player[i], result.egid[i], 1);
            }

            _indexedDB.Step();
            _submissionScheduler.SubmitEntities();

            AssertIndexer(0, -1, 1, out queriedComponents);

            Assert.Equal(5, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.entityID));

            AssertIndexer(0, 1, 1, out queriedComponents);

            Assert.Equal(15, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(5u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.entityID));
        }

        [Fact]
        public void QueryWithGroupTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(i, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(-1));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(i, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(3));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Item, itemIdCounter);
                itemBuilder.Init(new ItemOwnerComponent(i, itemIdCounter++));
                itemBuilder.Init(new PlayerComponent(5));
            }

            _submissionScheduler.SubmitEntities();

            foreach (var result in _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Item)
                .Where(_schema.ItemOwner.Is(0)).Where(_schema.Player.Is(-1)))
            {
                int count = 0;

                foreach (var i in result.indices)
                {
                    count++;
                    Assert.Equal(0u, result.set.itemOwner[i].entityID);
                }

                Assert.Equal(1, count);
            }

            AssertIndexer(7, null, 3, out var queriedComponents);

            Assert.Equal(3, queriedComponents.Count);

            Assert.Contains(7u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(17u, queriedComponents.Select(x => x.entityID));
            Assert.Contains(27u, queriedComponents.Select(x => x.entityID));

            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.entityID));
        }
    }
}
