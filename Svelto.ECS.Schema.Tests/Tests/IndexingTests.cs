using System;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class IndexingTests : SchemaTestsBase<IndexingTests.TestSchema>
    {
        public readonly struct ItemOwner : IEntityIndexKey<ItemOwner>
        {
            public readonly int characterId;

            public ItemOwner(int id) => characterId = id;

            public bool Equals(ItemOwner other) => characterId == other.characterId;
        }

        // descriptors
        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }

        public class ItemDescriptor : GenericEntityDescriptor<Indexed<ItemOwner>> { }

        // schemas
        public class PlayerSchema : IEntitySchema
        {
            internal Table<CharacterDescriptor> _character = new Table<CharacterDescriptor>();
            public Group<CharacterDescriptor> Character => _character.Group();

            internal Table<ItemDescriptor> _item = new Table<ItemDescriptor>();
            public Group<ItemDescriptor> Item => _item.Group();

            internal Index<ItemOwner> _itemsByOwner = new Index<ItemOwner>();
            public IndexQuery<ItemOwner> ItemsByOwner(int characterId) => _itemsByOwner.Query(new ItemOwner(characterId));
        }

        public class TestSchema : IEntitySchema
        {
            internal Shard<PlayerSchema> _ai = new Shard<PlayerSchema>();
            public PlayerSchema AI => _ai.Schema();

            internal Shard<PlayerSchema> _players = new Shard<PlayerSchema>(10);
            public PlayerSchema Player(int playerId) => _players.Schema(playerId);

            internal Index<ItemOwner> _itemsByOwner = new Index<ItemOwner>();
            public IndexQuery<ItemOwner> ItemsByOwner(int characterId) => _itemsByOwner.Query(new ItemOwner(characterId));

            public Tables<CharacterDescriptor> AllCharacters { get; }
            public Tables<ItemDescriptor> AllItems { get; }

            public TestSchema()
            {
                AllCharacters = AI.Character + _players.Schemas().Combine(x => x.Character);
                AllItems = AI.Item + _players.Schemas().Combine(x => x.Item);
            }
        }

        [Fact]
        public void IndexUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.AI.Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Player(0).Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Player(1).Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _schema.ItemsByOwner(0)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            var queriedComponents = queriedGroups
                .Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(3, queriedGroups.Count);
            Assert.Equal(30, queriedComponents.Count);

            Assert.All(queriedGroups, x => Assert.Equal(10, x.Item1.Item2.Count()));

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            Assert.All(queriedComponents.Select(x => x.Key), x => Assert.Equal(0, x.characterId));

            var (buffer, count) = _schema.Player(0).Item.Entities<Indexed<ItemOwner>>(_entitiesDB);
            for (int i = 0; i < count; ++i)
                buffer[i].Update(_indexesDB, new ItemOwner(1));

            queriedGroups = _schema.ItemsByOwner(0)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(2, queriedGroups.Count);
            Assert.Equal(20, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _schema.ItemsByOwner(1)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
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
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Player(0).Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Player(1).Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _schema.AI.ItemsByOwner(0)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            var queriedComponents = queriedGroups
                .Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(10, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));

            var (buffer, count) = _schema.AI.Item.Entities<Indexed<ItemOwner>>(_entitiesDB);
            for (int i = 0; i < count / 2; ++i)
                _functions.SwapEntityGroup(buffer[i].ID, _schema.Player(1).Item);

            _submissionScheduler.SubmitEntities();

            queriedGroups = _schema.AI.ItemsByOwner(0)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            queriedComponents = queriedGroups
                .Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(5, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _schema.Player(1).ItemsByOwner(0)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            queriedComponents = queriedGroups
                .Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
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
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Player(3).Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _schema.Player(5).Item.Build(_factory, itemIdCounter++);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
            }

            _submissionScheduler.SubmitEntities();

            // var (buffer, indices) = _schema.ItemsByOwner(0).From(_schema.AI.Item)).Entities<Indexed<ItemOwner>>();
            var (buffer, indices) = _schema.ItemsByOwner(0)
                .From(_schema.AI.Item)
                .Entities<Indexed<ItemOwner>>(_indexesDB);

            Assert.Equal(1, indices.Count());
            Assert.Equal(0u, buffer[indices[0]].ID.entityID);

            var queriedGroups = _schema.ItemsByOwner(7)
                .From(_schema.AllItems)
                .Entities<Indexed<ItemOwner>>(_indexesDB).ToList();

            var queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(3, queriedGroups.Count);
            Assert.Equal(3, queriedComponents.Count);

            Assert.All(queriedGroups, x => Assert.Equal(1, x.Item1.Item2.Count()));

            Assert.Contains(7u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(17u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(27u, queriedComponents.Select(x => x.ID.entityID));

            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));
        }
    }
}
