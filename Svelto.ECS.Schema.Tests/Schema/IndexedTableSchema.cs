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
    // keys
    public struct ItemOwner : IEntityIndexKey
    {
        public int characterId;

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
        static Table<ItemDescriptor> item = new Table<ItemDescriptor>();

        public ExclusiveGroupStruct Character => character.At(Offset).Group();
        public ExclusiveGroupStruct Item => item.At(Offset).Group();
    }

    // schema
    public class SampleSchema : IEntitySchema<SampleSchema>
    {
        public SchemaContext Context { get; set; }

        static Partition<PlayerShard> ai = new Partition<PlayerShard>();
        static Partition<PlayerShard> players = new Partition<PlayerShard>(10);

        static Index<ItemOwner> itemByOwner = new Index<ItemOwner>();

        public PlayerShard AI => ai.Shard();
        public PlayerShard Player(int playerId) => players.Shard(playerId);

        public IndexQuery ItemByOwner(int characterId) => itemByOwner.Query(characterId);

        public FasterList<ExclusiveGroupStruct> AllCharacters { get; }
        public FasterList<ExclusiveGroupStruct> AllItems { get; }

        public SampleSchema()
        {
            AllCharacters = Enumerable.Range(0, 10).Select(Player)
                .Append(AI).Select(x => x.Character).ToFasterList();

            AllItems = Enumerable.Range(0, 10).Select(Player)
                .Append(AI).Select(x => x.Item).ToFasterList();
        }
    }
}
