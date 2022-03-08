using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class IndexingTests : SchemaTestsBase<IndexingTests.TestSchema>
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

        // rows
        public class CharacterRow : DescriptorRow<CharacterRow>,
            IQueryableRow<EGIDSet>
        {}

        public class ItemRow : DescriptorRow<ItemRow>,
            IIndexableRow<ItemOwnerComponent>,
            IQueryableRow<ItemWithOwnerSet>
        {}

        // schemas
        public class PlayerSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new Table<CharacterRow>();
            public readonly Table<ItemRow> Item = new Table<ItemRow>();

            public readonly Index<ItemOwnerComponent> ItemOwner = new Index<ItemOwnerComponent>();
        }

        public class TestSchema : IEntitySchema
        {
            public readonly PlayerSchema AI = new PlayerSchema();
            public readonly Ranged<PlayerSchema> Players = new Ranged<PlayerSchema>(10);

            public readonly Index<ItemOwnerComponent> ItemOwner = new Index<ItemOwnerComponent>();

            public readonly CombinedTables<CharacterRow> AllCharacters;
            public readonly CombinedTables<ItemRow> AllItems;

            public TestSchema()
            {
                AllCharacters = AI.Character.Append(Players.Combine(x => x.Character));
                AllItems = Players.Combine(x => x.Item).Append(AI.Item);
            }
        }

        [Fact]
        public void IndexUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.AI.Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[0].Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[1].Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(0));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _indexedDB.Select<ItemWithOwnerSet>().FromAll<ItemRow>()
                .Where(_schema.ItemOwner.Is(0)).Entities().ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Equal(3, queriedGroups.Count);
            Assert.Equal(30, queriedComponents.Count);

            Assert.All(queriedGroups, x => Assert.Equal(10, x.indices.Count()));

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            Assert.All(queriedComponents.Select(x => x.key), x => Assert.Equal(0, x));

            var result = _indexedDB.Select<ItemWithOwnerSet>().From(_schema.Players[0].Item).Entities();
            foreach (var i in result.indices)
                _indexedDB.Update(ref result.set.itemOwner[i], 1);

            queriedGroups = _indexedDB.Select<ItemWithOwnerSet>()
                .FromAll<ItemRow>().Where(_schema.ItemOwner.Is(0)).Entities().ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Equal(2, queriedGroups.Count);
            Assert.Equal(20, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _indexedDB.Select<ItemWithOwnerSet>()
                .FromAll<ItemRow>().Where(_schema.ItemOwner.Is(1)).Entities().ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(10, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));
        }

        [Fact]
        public void GroupUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.AI.Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[0].Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[1].Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(0));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _indexedDB.Select<ItemWithOwnerSet>()
                .FromAll<ItemRow>().Where(_schema.AI.ItemOwner.Is(0)).Entities().ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(10, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));

            var result = _indexedDB.Select<ItemWithOwnerSet>().From(_schema.AI.Item).Entities();
            for (int i = 0; i < result.set.count / 2; ++i)
                _functions.Move(_schema.AI.Item, result.set.itemOwner[i].ID.entityID).To(_schema.Players[1].Item);

            _submissionScheduler.SubmitEntities();

            queriedGroups = _indexedDB.Select<ItemWithOwnerSet>()
                .FromAll<ItemRow>().Where(_schema.AI.ItemOwner.Is(0)).Entities().ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(5, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _indexedDB.Select<ItemWithOwnerSet>()
                .FromAll<ItemRow>().Where(_schema.Players[1].ItemOwner.Is(0)).Entities().ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(15, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(5u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));
        }

        [Fact]
        public void QueryWithGroupTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.AI.Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[3].Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[5].Item, itemIdCounter++);
                itemBuilder.Init(new ItemOwnerComponent(i));
            }

            _submissionScheduler.SubmitEntities();

            var result = _indexedDB.Select<ItemWithOwnerSet>()
                .From(_schema.AI.Item).Where(_schema.ItemOwner.Is(0)).Entities();

            Assert.Equal(1, result.indices.Count());
            Assert.Equal(0u, result.set.itemOwner[result.indices[0]].ID.entityID);

            var queriedGroups = _indexedDB.Select<ItemWithOwnerSet>()
                .From(_schema.AllItems).Where(_schema.ItemOwner.Is(7)).Entities().ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.set.itemOwner[x.indices[i]]))
                .ToList();

            Assert.Equal(3, queriedGroups.Count);
            Assert.Equal(3, queriedComponents.Count);

            Assert.All(queriedGroups, x => Assert.Equal(1, x.indices.Count()));

            Assert.Contains(7u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(17u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(27u, queriedComponents.Select(x => x.ID.entityID));

            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));
        }
    }
}
