using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Schema.Definition;

namespace Svelto.ECS.Schema.Tests
{
    public readonly struct ItemOwner : IEntityIndexKey
    {
        public readonly int characterId;

        public int Key => characterId;

        public ItemOwner(int id)
        {
            characterId = id;
        }
    }

    // descriptors
    public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }

    public class ItemDescriptor : GenericEntityDescriptor<Indexed<ItemOwner>> { }

    // partition
    public struct PlayerShard : IEntityShard
    {
        public ShardOffset Offset { get; set; }

        static Table<CharacterDescriptor> character = new Table<CharacterDescriptor>();
        public Group<CharacterDescriptor> Character => character.At(Offset).Group();

        static Table<ItemDescriptor> item = new Table<ItemDescriptor>();
        public Group<ItemDescriptor> Item => item.At(Offset).Group();
    }

    // schema
    public class SampleSchema : IEntitySchema<SampleSchema>
    {
        public SchemaContext Context { get; set; }

        static Partition<PlayerShard> ai = new Partition<PlayerShard>();
        public PlayerShard AI => ai.Shard();

        static Partition<PlayerShard> players = new Partition<PlayerShard>(10);
        public PlayerShard Player(int playerId) => players.Shard(playerId);

        static Index<ItemOwner> itemsByOwner = new Index<ItemOwner>();
        public IndexQuery ItemsByOwner(int characterId) => itemsByOwner.Query(characterId);

        public Groups<CharacterDescriptor> AllCharacters => AI.Character + players.Shards().Each(x => x.Character);
        public Groups<ItemDescriptor> AllItems => AI.Item + players.Shards().Each(x => x.Item);
    }
}
