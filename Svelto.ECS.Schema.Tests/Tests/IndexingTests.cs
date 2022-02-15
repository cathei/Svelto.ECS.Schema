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

        // partition
        public struct PlayerShard : IEntityShard
        {
            public ShardOffset Offset { get; set; }

            internal static Table<CharacterDescriptor> character = new Table<CharacterDescriptor>();
            public Group<CharacterDescriptor> Character => character.At(Offset).Group();

            internal static Table<ItemDescriptor> item = new Table<ItemDescriptor>();
            public Group<ItemDescriptor> Item => item.At(Offset).Group();

            internal static Index<ItemOwner> itemsByOwner = new Index<ItemOwner>();
            public IndexQuery<ItemOwner> ItemsByOwner(int characterId) => itemsByOwner.At(Offset).Query(new ItemOwner(characterId));
        }

        // schema
        public class TestSchema : IEntitySchema<TestSchema>
        {
            public SchemaContext Context { get; set; }

            internal static Partition<PlayerShard> ai = new Partition<PlayerShard>();
            public PlayerShard AI => ai.Shard();

            internal static Partition<PlayerShard> players = new Partition<PlayerShard>(10);
            public PlayerShard Player(int playerId) => players.Shard(playerId);

            internal static Index<ItemOwner> itemsByOwner = new Index<ItemOwner>();
            public IndexQuery<ItemOwner> ItemsByOwner(int characterId) => itemsByOwner.Query(new ItemOwner(characterId));

            public Groups<CharacterDescriptor> AllCharacters => AI.Character + players.Shards().Each(x => x.Character);
            public Groups<ItemDescriptor> AllItems => AI.Item + players.Shards().Each(x => x.Item);
        }

        [Fact]
        public void IndexUpdateTest()
        {
            uint itemIdCounter = 0;

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.AI.Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.Player(0).Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.Player(1).Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _schema.ItemsByOwner(0).QueryEntities<Indexed<ItemOwner>>(_schema.Context).ToList();
            var queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(3, queriedGroups.Count);
            Assert.Equal(30, queriedComponents.Count);

            Assert.All(queriedGroups, x => Assert.Equal(10, x.Item1.Item2.Count()));

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            Assert.All(queriedComponents.Select(x => x.Key), x => Assert.Equal(0, x.characterId));

            var (buffer, count) = _entitiesDB.QueryEntities<Indexed<ItemOwner>>(_schema.Player(0).Item);
            for (int i = 0; i < count; ++i)
                buffer[i].Update(_schema.Context, new ItemOwner(1));

            queriedGroups = _schema.ItemsByOwner(0).QueryEntities<Indexed<ItemOwner>>(_schema.Context).ToList();
            queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Equal(2, queriedGroups.Count);
            Assert.Equal(20, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(20u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _schema.ItemsByOwner(1).QueryEntities<Indexed<ItemOwner>>(_schema.Context).ToList();
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
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.AI.Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.Player(0).Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.Player(1).Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(0)));
            }

            _submissionScheduler.SubmitEntities();

            var queriedGroups = _schema.AI.ItemsByOwner(0).QueryEntities<Indexed<ItemOwner>>(_schema.Context).ToList();
            var queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(10, queriedComponents.Count);

            Assert.Contains(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(10u, queriedComponents.Select(x => x.ID.entityID));
            Assert.DoesNotContain(20u, queriedComponents.Select(x => x.ID.entityID));

            var (buffer, count) = _entitiesDB.QueryEntities<Indexed<ItemOwner>>(_schema.AI.Item);
            for (int i = 0; i < count / 2; ++i)
                _functions.SwapEntityGroup(buffer[i].ID, _schema.Player(1).Item);

            _submissionScheduler.SubmitEntities();

            queriedGroups = _schema.AI.ItemsByOwner(0).QueryEntities<Indexed<ItemOwner>>(_schema.Context).ToList();
            queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
                .SelectMany(x => Enumerable.Range(0, x.indices.Count()).Select(i => x.buffer[x.indices[i]]))
                .ToList();

            Assert.Single(queriedGroups);
            Assert.Equal(5, queriedComponents.Count);

            Assert.DoesNotContain(0u, queriedComponents.Select(x => x.ID.entityID));
            Assert.Contains(5u, queriedComponents.Select(x => x.ID.entityID));

            queriedGroups = _schema.Player(1).ItemsByOwner(0).QueryEntities<Indexed<ItemOwner>>(_schema.Context).ToList();
            queriedComponents = queriedGroups.Select(x => (buffer: x.Item1.Item1, indices : x.Item1.Item2))
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
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.AI.Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.Player(3).Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
            }

            for (int i = 0; i < 10; ++i)
            {
                var itemBuilder = _factory.BuildEntity(itemIdCounter++, _schema.Player(5).Item);
                itemBuilder.Init(new Indexed<ItemOwner>(new ItemOwner(i)));
            }

            _submissionScheduler.SubmitEntities();

            var (buffer, indices) = _schema.ItemsByOwner(0).QueryEntities<Indexed<ItemOwner>>(_schema.Context, _schema.AI.Item);

            Assert.Equal(1, indices.Count());
            Assert.Equal(0u, buffer[indices[0]].ID.entityID);

            var queriedGroups = _schema.ItemsByOwner(7).QueryEntities<Indexed<ItemOwner>>(_schema.Context, _schema.AllItems.Build()).ToList();
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
