using System.Linq;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class IndexingTests : SchemaTestsBase<IndexingTests.TestSchema>
    {
        public interface IHaveEGID : IEntityRow<EGIDComponent> { }

        public interface IIndexedItemOwner : IIndexedRow<int, IIndexedItemOwner.Tag>
        { public struct Tag : ITag {} }

        // rows
        public class CharacterRow : DescriptorRow<CharacterRow>, IHaveEGID {}

        public class ItemRow : DescriptorRow<ItemRow>, IIndexedItemOwner {}

        // schemas
        public class PlayerSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new Table<CharacterRow>();
            public readonly Table<ItemRow> Item = new Table<ItemRow>();

            public readonly IIndexedItemOwner.Index ItemOwner = new IIndexedItemOwner.Index();
        }

        public class TestSchema : IEntitySchema
        {
            public readonly PlayerSchema AI = new PlayerSchema();
            public readonly Ranged<PlayerSchema> Players = new Ranged<PlayerSchema>(10);

            public readonly IIndexedItemOwner.Index ItemOwner = new IIndexedItemOwner.Index();

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
                itemBuilder.Init(new IIndexedItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[0].Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[1].Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(0));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _indexedDB.Select<IIndexedItemOwner>().All()
                .Where(_schema.ItemOwner, 0).Entities().ToList();

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

            var (owner, count) = _indexedDB.Select<IIndexedItemOwner>().From(_schema.Players[0].Item).Entities();
            for (int i = 0; i < count; ++i)
                _indexedDB.Update(ref owner[i], 1);

            queriedGroups = _indexedDB.Select<IIndexedItemOwner>()
                .All().Where(_schema.ItemOwner, 0).Entities().ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(2, queriedGroups.Count);
            Assert.Equal(20, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _indexedDB.Select<IIndexedItemOwner>()
                .All().Where(_schema.ItemOwner, 1).Entities().ToList();

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
                var itemBuilder = _factory.Build(_schema.AI.Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[0].Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(0));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[1].Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(0));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _indexedDB.Select<IIndexedItemOwner>()
                .All().Where(_schema.AI.ItemOwner, 0).Entities().ToList();

            var queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(10, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));

            var (buffer, count) = _indexedDB.Select<IIndexedItemOwner>().From(_schema.AI.Item).Entities();
            for (int i = 0; i < count / 2; ++i)
                _functions.Move(_schema.AI.Item, buffer[i].ID.entityID).To(_schema.Players[1].Item);

            _submissionScheduler.SubmitEntities();

            queriedGroups = _indexedDB.Select<IIndexedItemOwner>()
                .All().Where(_schema.AI.ItemOwner, 0).Entities().ToList();

            queriedComponents = queriedGroups
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(5, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _indexedDB.Select<IIndexedItemOwner>()
                .All().Where(_schema.Players[1].ItemOwner, 0).Entities().ToList();

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
                var itemBuilder = _factory.Build(_schema.AI.Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[3].Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.Build(_schema.Players[5].Item, itemIdCounter++);
                itemBuilder.Init(new IIndexedItemOwner.Component(i));
            }

            _submissionScheduler.SubmitEntities();

            var ((buffer, count), indices) = _indexedDB.Select<IIndexedItemOwner>()
                .From(_schema.AI.Item).Where(_schema.ItemOwner, 0).Entities();

            Assert.Equal(1, indices.Count());
            Assert.Equal(0u, buffer[indices[0]].ID.entityID);

            var queriedGroups = _indexedDB.Select<IIndexedItemOwner>()
                .From(_schema.AllItems).Where(_schema.ItemOwner, 7).Entities().ToList();

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
