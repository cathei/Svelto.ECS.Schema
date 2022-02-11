## Svelto.ECS.Schema
Extension for Svelto.ECS, helps defining structure like database schema.

### Motivation
Svelto.ECS is an awesome project, however I found understanding underlying entity structure can be pretty confusing to new users like myself.
It has powerful tools like groups and filters, but lacks of wrapper to make it intutive.
I thought it will be much easier to understand group with structured schema, and it is worth to make your code flexible, design change proof.
That is the motivation I wrote this Svelto.ECS.Schema extension which is basically a user-friendly wrapper for groups and filters.

### Concept
Think of a RDBMS schema, there is tables, records, columns, indexes and partitions. ECS is basically in-memory database but faster.
In RDBMS, tables can hold records having specific combination of columns.
In Svelto.ECS, groups can hold entities having specific combination of components.
That is why I chose to take friendly terms from RDBMS and define schema of ECS.

### Basic Usage
##### Install
Currently it is PoC state. So I didn't publish anywhere but feel free to try it by cloning this repository and let me know how you feel!

##### Defining Descriptor
Let's say you have basic understanding of general Entity-Component-System.
Defining schema starts from defining EntityDescriptor, that is combination of components.
```csharp
public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent, HealthComponent, PositionComponent> { }
```
It is not part of this extension, but it is important because it is basically definition of records that table can hold.

##### Defining Schema
Let's define simplest Schema.
```csharp
public class GameSchema : IEntitySchema<GameSchema>
{
    public SchemaContext Context { get; set; }

    static Table<CharacterDescriptor> character = new Table<CharacterDescriptor>();
    static Table<ItemDescriptor> item = new Table<ItemDescriptor>();

    public ExclusiveGroupStruct Character => character.Group();
    public ExclusiveGroupStruct Item => item.Group();
}
```
`IEntitySchema<TSelf>` represents a class that will hold all defined tables as member. `Context` property is requried by `IEntitySchema<TSelf>`.

`Table<TDescriptor>` represents underlying `ExclusiveGroup`. Tables should only accept entities using same descriptor, or else the index will break.

Note that tables are defined as static, and only underlying `ExclusiveGroupStruct` are exposed. This is pattern I recommend, so rest of your code can be kept clean and have minimum dependency to Schema extension.

##### Defining Multiple Tables
Sometimes you'll want many tables of same type, without defining many variables. Simiply pass the number of group you want to be created, and there are multiple separated groups!
```csharp
public class AnotherSchema : IEntitySchema<AnotherSchema>
{
    public SchemaContext Context { get; set; }

    public const int MaxPlayerCount = 10;
    public enum ItemType { Potion, Weapon, Armor, MAX };

    static Table<PlayerDescriptor> players = new Table<PlayerDescriptor>(MaxPlayerCount);
    static Table<ItemDescriptor> items = new Table<ItemDescriptor>((int)ItemType.MAX);

    public ExclusiveGroupStruct Player(int playerId) => players.Group(playerId);
    public ExclusiveGroupStruct Items(ItemType type) => items.Group((int)type);

    public FasterList<ExclusiveGroupStruct> AllPlayers { get; }

    public AnotherSchema()
    {
        AllPlayers = Enumerable.Range(0, MaxPlayerCount).Select(Player).ToFasterList();
    }
}
```
Above example shows use case of multiple tables with number or enum. Note that `AnotherSchema.AllPlayers` array caches all player group so you can directly pass it to `EntitiesDB.QueryEntities`. Do not make them static, it is not safe to access to schema until it is added to `EnginesRoot`.

##### Applying Schema
Before we can use schema, we need to call `EnginesRoot.AddSchema<T>()`. When you initializing `EnginesRoot`, do this before any entitiy submission.
```csharp
MyGameSchema schema = enginesRoot.AddSchema<MyGameSchema>();
```

Now it's time to fill up your tables with records.
```csharp
public class CompositionRoot
{
    private uint eidCounter = 0;

    public CompositionRoot()
    {
        var submissionScheduler = new SimpleEntitiesSubmissionScheduler();
        var enginesRoot = new EnginesRoot(submissionScheduler);

        var entityFactory = enginesRoot.GenerateEntityFactory();
        var schema = enginesRoot.AddSchema<MyGameSchema>();

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, schema.AI.AliveCharacter);

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, schema.Player(0).DeadCharacter);

        submissionScheduler.SubmitEntities();
    }

    private void AddCharacter(IEntityFactory entityFactory, ExclusiveGroupStruct group)
    {
        var builder = entityFactory.BuildEntity<CharacterDescriptor>(eidCounter++, group);

        builder.Init(new HealthComponent(1000));
        builder.Init(new PositionComponent(0, 0));
    }
}
```
Above we have example to put 10 characters to alive, AI controlled character group, and put another 10 characters to dead, player 0 controlled character group.

Inject schema to your engines. Now you can query this from your desired engine.
```csharp
foreach (var ((healths, positions, count), group) in entitiesDB.QueryEntities<HealthComponent, PositionComponent>(schema.AllAliveCharacters))
{
    for (int i = 0; i < count; ++i)
    {
        healths[i].current -= 100;
    }
}
```

##### Defining Partition
On the other hand, you will want to group some related tables, and reuse it. We use `Partition<TShard>` for it. First, define a shard, which is logical group of tables.
```csharp
public struct PlayerShard : IEntityShard
{
    public ShardOffset Offset { get; set; }

    public enum ItemType { Potion, Weapon, Armor, MAX };

    static Table<CharacterDescriptor> aliveCharacter = new Table<CharacterDescriptor>();
    static Table<CharacterDescriptor> deadCharacter = new Table<CharacterDescriptor>();
    static Table<ItemDescriptor> items = new Table<ItemDescriptor>((int)ItemType.MAX);

    public ExclusiveGroupStruct AliveCharacter => aliveCharacter.At(Offset).Group();
    public ExclusiveGroupStruct DeadCharacter => deadCharacter.At(Offset).Group();
    public ExclusiveGroupStruct Items(ItemType type) => items.At(Offset).Group((int)type);
}
```
Looks similar to defining schema, but little different. First, `PlayerShard` is `struct`. Second, there is `Offset` property which defined in IEntityShard, and they are passed in properties by `.At(Offset)` before get the underlying group.

Reason of this is `PlayerShard` can be reused but tables will be static. So we need to pass a `Offset` information to get the correct group when accessed from different partition.

Now we have Shard, we can define actual partition with `Partition<TShard>` in the schema.

```csharp
public class MyGameSchema : IEntitySchema<MyGameSchema>
{
    public const int MaxPlayerCount = 10;

    public SchemaContext Context { get; set; }

    static Partition<PlayerShard> ai = new Partition<PlayerShard>();
    static Partition<PlayerShard> players = new Partition<PlayerShard>(MaxPlayerCount);

    public PlayerShard AI => ai.Shard();
    public PlayerShard Player(int playerId) => players.Shard(playerId);

    public static FasterList<ExclusiveGroupStruct> AllAliveCharacters { get; }

    public MyGameSchema()
    {
        AllAliveCharacters = Enumerable.Range(0, MaxPlayerCount)
            .Select(Player).Append(AI)
            .Select(x => x.AliveCharacter).ToFasterList();
    }
}
```
Nice. We defined a group for AI, and 10 players. Just like how we expose group instead of table, we'll expose shard insted of partition. If you want to access group for player 5's alive characters, use `MyGameSchema.Player(5).AliveCharacter`. Also we added shortcut groups for all alive characters.

##### vs. Doofuses example
GroupCompound is good enough for simple, static groups. But not all the groups in game is simple or static. Most of them are not, actually. Let's look at the Doofuses example of Svelto.ECS.MiniExamples. They have groups like this.
```csharp
static class GameGroups
{
    public class DOOFUSES : GroupTag<DOOFUSES> { }
    public class FOOD : GroupTag<FOOD> { }
    
    public class RED : GroupTag<RED> { }
    public class BLUE : GroupTag<BLUE> { }
    
    public class EATING : GroupTag<EATING> { }
    public class NOTEATING : GroupTag<NOTEATING> { }

    public class RED_DOOFUSES_EATING : GroupCompound<DOOFUSES, RED, EATING> { };
    public class RED_DOOFUSES_NOT_EATING :  GroupCompound<DOOFUSES, RED, NOTEATING> { };
    public class RED_FOOD_EATEN : GroupCompound<FOOD, RED, EATING> { };
    public class RED_FOOD_NOT_EATEN : GroupCompound<FOOD, RED, NOTEATING> { };
    
    public class BLUE_DOOFUSES_EATING : GroupCompound<DOOFUSES, BLUE, EATING> { };
    public class BLUE_DOOFUSES_NOT_EATING :  GroupCompound<DOOFUSES, BLUE, NOTEATING> { };
    public class BLUE_FOOD_EATEN : GroupCompound<FOOD, BLUE, EATING> { };
    public class BLUE_FOOD_NOT_EATEN : GroupCompound<FOOD, BLUE, NOTEATING> { };

    public class DOOFUSES_EATING : GroupCompound<DOOFUSES, EATING> { };
}
```
There is entity type of Doofuses and Food, team of Red and Blue, state of Eating and NonEating. And groups are made with their combinations. I think it will be easy if you get used to it, but little confusing to understand structure at the first.

Real problem is it is not really flexible nor extendible. What if Yellow team is needed? What if state of Flying and Ground is needed? We'll have to define all the combinations we need. Game design will change over time, and I think it is not managable through GroupCompound at some point. 

With Schema extension this would be converted to below.
```csharp
public struct StateShard : IEntityShard
{
    public ShardOffset Offset { get; set; }

    static Table<DoofusEntityDescriptor> doofus = new Table<DoofusEntityDescriptor>();
    static Table<FoodEntityDescriptor> food = new Table<FoodEntityDescriptor>();

    public ExclusiveGroupStruct Doofus => doofus.At(Offset).Group();
    public ExclusiveGroupStruct Food => food.At(Offset).Group();
}

public struct TeamShard : IEntityShard
{
    public ShardOffset Offset { get; set; }

    static Partition<StateShard> eating = new Partition<StateShard>();
    static Partition<StateShard> nonEating = new Partition<StateShard>();

    public StateShard Eating => eating.At(Offset).Shard();
    public StateShard NonEating => nonEating.At(Offset).Shard();
}

public enum TeamColor { Red, Blue, MAX }

public class GameSchema : IEntitySchema<GameSchema>
{
    public SchemaContext Context { get; set; }

    static Partition<TeamShard> team = new Partition<TeamShard>((int)TeamColor.MAX);

    public TeamShard Team(TeamColor color) => team.Shard((int)color);

    public FasterList<ExclusiveGroupStruct> EatingDoofuses { get; }

    public GameSchema()
    {
        EatingDoofuses = Enumerable.Range(0, (int)TeamColor.MAX)
            .Select(x => team.Shard(x).Eating.Doofus)
            .ToFasterList();
    }
}
```
More code, but you'll thank to some complexity when you have to deal with big design changes!

When using it, code `GameGroups.RED_DOOFUSES_EATING.Groups` would be equvalent to `GameSchema.Team(TeamColor.Red).Eating.Doofus`.

### Defining Indexes
Index is wrapper of filters system, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a partition or entire schema. Let's take a look. You have to define Key first.
```csharp
public struct Controller : IEntityIndexKey
{
    public int playerId;

    public int Key => playerId;

    public Controller(int playerId)
    {
        this.playerId = playerId;
    }
}
```
Keys are structs inheriting `IEntityIndexKey`. You can have more members in it, but you need to provide which member or combination will be `Key`. If the type is not `int`, consider using `GetHashCode()`.

Then, you add `Indexed<TKey>` to your descriptor.

```csharp
public class CharacterDescriptor<HealthComponent, PositionComponent, Indexed<Controller>> { }
```
Indexed is special component to make sure that indexes are up-to-date. It has Controller struct as member `Content`, but you cannot change the value unless you use `Indexed<TKey>.Update(SchemaContext, TKey)`. `SchemaContext` is accessable with `IEntitySchema<T>.Context` and represents current state of entity, like indexing.

Before look into `SchemaContext`, Let's add `Index<TKey>` to our schema.
```csharp
public class IndexedSchema : IEntitySchema<IndexedSchema>
{
    static Table<CharacterDescriptor> flyingCharacter = new Table<CharacterDescriptor>();
    static Table<CharacterDescriptor> groundCharacter = new Table<CharacterDescriptor>();

    static Index<Controller> characterByController = new Index<Controller>();

    public ExclusiveGroupStruct FlyingCharacter => flyingCharacter.Group();
    public ExclusiveGroupStruct GroundCharacter => flyingCharacter.Group();

    public IndexQuery CharacterByController(int playerId) => characterByController.Query(playerId);
}
```
`Index<TKey>` will index any `Indexed<TKey>` component in any tables with same partition. Any child partition will be indexed as well. If `Index<TKey>` is defined in root schema, any table with `Indexed<TKey>` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` group will be indexed and returned when queried. If you want to index specific groups only, define a partition.

Same manner as we expose a group for a table, we'll expose `IndexQuery` for a index. `IndexQuery` is query for a specific key, like 'player id 0'.

Though there is no constraint yet, it is not recommended to share `Indexed<TKey>` across other descriptors.

##### Querying Indexes
Now, finally you can iterate over entities with `IndexQuery`. You don't have to include `Indexed<TKey>` in the query. You can query any type of component within the descriptor, because as long as you keep a group with single descriptor you can iterate with same filter.

Just like when you query with `EntitiesDB`, you query with `SchemaContext`.
```csharp
foreach (var ((health, position, indices), group) in schema.Context.QueryEntities<HealthComponent, PositionComponent>(IndexedSchema.CharacterByController(3)))
{
    for (int i = 0; i < indices.count(); ++i)
    {
        health[indices[i]].current += 10;
    }
}
```
Note that you have to use double indexing like `health[indices[i]]`.
