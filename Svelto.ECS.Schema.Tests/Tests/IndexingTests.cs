using System.Linq;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class IndexingTests : SchemaTestsBase<IndexingTests.TestSchema>
    {
        public class ItemOwner : IndexTag<int, ItemOwner.Unique>
        {
            public struct Unique : IUnique {}
        }

        // descriptors
        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }

        public class ItemDescriptor : GenericEntityDescriptor<ItemOwner.Component> { }

        // schemas
        public class PlayerSchema : IEntitySchema
        {
            public readonly Table<CharacterDescriptor> Character = new Table<CharacterDescriptor>();
            public readonly Table<ItemDescriptor> Item = new Table<ItemDescriptor>();

            public readonly ItemOwner.Index ItemsByOwner = new ItemOwner.Index();
        }

        public class TestSchema : IEntitySchema
        {
            public readonly PlayerSchema AI = new PlayerSchema();
            public readonly Ranged<PlayerSchema> Players = new Ranged<PlayerSchema>(10);

            public readonly ItemOwner.Index ItemsByOwner = new ItemOwner.Index();

            public readonly Tables<CharacterDescriptor> AllCharacters;
            public readonly Tables<ItemDescriptor> AllItems;

            public TestSchema()
            {
                AllCharacters = AI.Character + Players.Combine(x => x.Character);
                AllItems = AI.Item + Players.Combine(x => x.Item);
            }
        }

        [Fact]
        public void IndexUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.AI.Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Players[0].Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Players[1].Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(0));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _schema.ItemsByOwner.Query(0)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(3, queriedGroups.Count);
            Assert.Equal(30, queriedComponents.Count);

            Assert.All(queriedGroups, x => Assert.Equal(10, x.indices.Count()));

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            Assert.All(queriedComponents.Select(x => x.Value), x => Assert.Equal(0, x));

            var (owner, count) = _schema.Players[0].Item.Entities<ItemOwner.Component>(_indexedDB);
            for (int i = 0; i < count; ++i)
                owner[i].Update(_indexedDB, 1);

            queriedGroups = _schema.ItemsByOwner.Query(0)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(2, queriedGroups.Count);
            Assert.Equal(20, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _schema.ItemsByOwner.Query(1)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
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
                var itemBuilder = _schema.AI.Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Players[0].Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Players[1].Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(0));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _schema.AI.ItemsByOwner.Query(0)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(10, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));

            var (buffer, count) = _schema.AI.Item.Entities<ItemOwner.Component>(_indexedDB);
            for (int i = 0; i < count / 2; ++i)
                _functions.SwapEntityGroup(buffer[i].ID, _schema.Players[1].Item);

            _submissionScheduler.SubmitEntities();

            queriedGroups = _schema.AI.ItemsByOwner.Query(0)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(5, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _schema.Players[1].ItemsByOwner.Query(0)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
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
                var itemBuilder = _schema.AI.Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Players[3].Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Players[5].Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new ItemOwner.Component(i));
            }

            _submissionScheduler.SubmitEntities();

            var ((buffer, count), indices) = _schema.ItemsByOwner.Query(0)
                .From(_schema.AI.Item)
                .Entities<ItemOwner.Component>(_indexedDB);

            Assert.Equal(1, indices.Count());
            Assert.Equal(0u, buffer[indices[0]].ID.entityID);

            var queriedGroups = _schema.ItemsByOwner.Query(7)
                .From(_schema.AllItems)
                .Entities<ItemOwner.Component>(_indexedDB).ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
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
