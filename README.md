[![Nuget](https://img.shields.io/nuget/v/Svelto.ECS.Schema)](https://www.nuget.org/packages/Svelto.ECS.Schema/) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/Dvak3QMj3n)

## Svelto.ECS.Schema
Extension for [Svelto.ECS](https://github.com/sebas77/Svelto.ECS), helps defining structure like database schema.

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

## Basic Usage
### Install
Currently it is alpha stage, available on [NuGet](https://www.nuget.org/packages/Svelto.ECS.Schema/). While I don't recommend to use it on production, feel free to try it and please share me the experience! 

### Need help?
If you need help or want to give feedback, you can either join [my Discord Channel](https://discord.gg/Dvak3QMj3n) or ping @cathei from [Svelto's Official Discord Channel](https://discord.gg/3qAdjDb).

### Defining Descriptor
Let's say you have basic understanding of general Entity-Component-System.
Defining schema starts from defining EntityDescriptor, that is combination of components.
```csharp
public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent, HealthComponent, PositionComponent> { }
```
It is not part of this extension, but it is important because it is basically definition of records that table can hold.

### Defining Schema
Let's define simplest Schema.
```csharp
public class GameSchema : IEntitySchema
{
    private static Table<CharacterDescriptor> _character = new Table<CharacterDescriptor>();
    public static Group<CharacterDescriptor> Character => _character.Group();

    private static Table<ItemDescriptor> _item = new Table<ItemDescriptor>();
    public static Group<ItemDescriptor> Item => _item.Group();
}
```
`IEntitySchema` represents a class that will hold all defined tables as static member.

`Table<TDescriptor>` represents underlying `ExclusiveGroup`. Groups should only accept entities using same descriptor, or else the iteration index will break. In Schema extension Table has Descriptor type argument, basically preventing the issue.

Note that tables are defined as static, and only `Group<T>` are exposed. This is pattern I recommend, so rest of your code can be kept clean and reslove all schema define related code in schema class.

Now to add entity with `Group<T>`, we support two ways. One is original Svelto way:
```csharp
var entityBuilder = entityFactory.BuildEntity(entityId, GameSchema.Character);
```
The other is Schema extension way (preferred):
```csharp
var entityBuilder = GameSchema.Character.Build(entityFactory, entityId);
```
Results are the same so it is just different expression.

To query entities of `Group<T>`, also original Svelto way:
```csharp
var (egid, count) = entitiesDB.QueryEntities<EGIDComponent>(GameSchema.Character);
```
and Schema extension way (preferred):
```csharp
var (egid, count) = GameSchema.Character.Entities<EGIDComponent>(entitiesDB);
```

Reason I decided to have different expression from original Svelto is because we can pass more type information to calls without making it look awkward. C# does not support partial generic type inference, and we don't wanna call like `QueryEntities<EGIDCompoent, CharacterDescriptor>(GameSchema.Character)`.

### Defining Ranged Table
Sometimes you'll want many tables of same type, without defining many variables. Simiply pass the number of group you want to be created, and there are multiple separated tables!
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class AnotherSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    private static Table<ItemDescriptor> _items = new Table<ItemDescriptor>((int)ItemType.MAX);
    public static Group<ItemDescriptor> Items(ItemType type) => _items.Group((int)type);

    private static Table<PlayerDescriptor> _players = new Table<PlayerDescriptor>(MaxPlayerCount);
    public static Group<PlayerDescriptor> Player(int playerId) => _players.Group(playerId);

    public static Groups<PlayerDescriptor> AllPlayers { get; } = _players.Groups();
}
```
Above example shows use case of ranged tables with number or enum.

Note that we also exposes `AnotherSchema.AllPlayers` which represents all player groups. `Groups<T>` has underlying `FasterList<ExclusiveGroupStruct>`. Which means you can directly pass it into `EntitiesDB.QueryEntities`. 
```csharp
foreach (var (...) in entitiesDB.QueryEntities<...>(AnotherSchema.AllPlayers)) { }
```
Or, in Schema extension way (preferred):
```csharp
foreach (var (...) in AnotherSchema.AllPlayers.Entities<...>(entitiesDB)) { }
```

### Defining Partition
On the other hand, you will want to group some related tables, and reuse it. We use `Partition<TShard>` for it. First, define a shard, which is logical group of tables.
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class PlayerShard : IEntityShard
{
    private Table<CharacterDescriptor> _aliveCharacter = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> AliveCharacter => _aliveCharacter.Group();

    private Table<CharacterDescriptor> _deadCharacter = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> DeadCharacter => _deadCharacter.Group();

    private Table<ItemDescriptor> _items = new Table<ItemDescriptor>((int)ItemType.MAX);
    public Group<ItemDescriptor> Item(ItemType type) => _items.Group((int)type);
}
```
Looks similar to defining schema, except shards should implement IEntityShard and all the fields are non-static.

Now we have Shard, we can define actual partition with `Partition<TShard>` in the schema.

```csharp
public class MyGameSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    private static Partition<PlayerShard> _ai = new Partition<PlayerShard>();
    public static PlayerShard AI => _ai.Shard();

    private static Partition<PlayerShard> _players = new Partition<PlayerShard>(MaxPlayerCount);
    public static PlayerShard Player(int playerId) => _players.Shard(playerId);

    public static Groups<CharacterDescriptor> AllAliveCharacters { get; } =
        AI.AliveCharacter + players.Shards().Each(x => x.AliveCharacter);
}
```
Nice. We defined a group for AI, and 10 players. Just like how we expose group instead of table, we'll expose shard insted of partition. If you want to access group for player 5's alive characters, use `MyGameSchema.Player(5).AliveCharacter`. Also we added shortcut groups for all alive characters.

### Applying Schema
Before we can use schema, we need to call `EnginesRoot.AddSchema<T>()`. When you initializing `EnginesRoot`, do this before any entitiy submission.
```csharp
SchemaContext schemaContext = enginesRoot.AddSchema<MyGameSchema>();
```
Return value, `SchemaContext` will be used to save runtime schema data like indexing.

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
        var schemaContext = enginesRoot.AddSchema<MyGameSchema>();

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, MyGameSchema.AI.AliveCharacter);

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, MyGameSchema.Player(0).DeadCharacter);

        submissionScheduler.SubmitEntities();
    }

    private void AddCharacter(IEntityFactory entityFactory, Group<CharacterDescriptor> group)
    {
        var builder = group.Build(entityFactory, eidCounter++);

        builder.Init(new HealthComponent(1000));
        builder.Init(new PositionComponent(0, 0));
    }
}
```
Above we have example to put 10 characters to alive, AI controlled character group, and put another 10 characters to dead, player 0 controlled character group. You don't have to specify descriptor when call BuildEntity, because group is already implying descriptor type.
```csharp
foreach (var ((healths, positions, count), group) in MyGameSchema.AllAliveCharacters.Entities<HealthComponent, PositionComponent>(entitiesDB))
{
    for (int i = 0; i < count; ++i)
    {
        healths[i].current -= 100;
    }
}
```

### vs. Doofuses example
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
public class StateShard : IEntityShard
{
    private Table<DoofusEntityDescriptor> _doofus = new Table<DoofusEntityDescriptor>();
    public Group<DoofusEntityDescriptor> Doofus => _doofus.Group();

    private Table<FoodEntityDescriptor> _food = new Table<FoodEntityDescriptor>();
    public Group<FoodEntityDescriptor> Food => _food.Group();
}

public class TeamShard : IEntityShard
{
    private Partition<StateShard> _eating = new Partition<StateShard>();
    public StateShard Eating => _eating.Shard();

    private Partition<StateShard> _nonEating = new Partition<StateShard>();
    public StateShard NonEating => _nonEating.Shard();
}

public enum TeamColor { Red, Blue, MAX }

public class GameSchema : IEntitySchema
{
    private static Partition<TeamShard> _team = new Partition<TeamShard>((int)TeamColor.MAX);
    public static TeamShard Team(TeamColor color) => _team.Shard((int)color);

    public static Groups<DoofusEntityDescriptor> EatingDoofuses { get; } = _team.Shards().Combine(x => x.Eating.Doofus);
}
```
More code, but you'll thank to some complexity when you have to deal with big design changes!

When using it, code `GameGroups.RED_DOOFUSES_EATING.Groups` would be equvalent to `GameSchema.Team(TeamColor.Red).Eating.Doofus`.

### Defining Indexes
Index is wrapper of filters system, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a partition or entire schema. Let's take a look. You have to define Key first.
```csharp
public readonly struct CharacterController : IEntityIndexKey<CharacterController>
{
    public readonly int PlayerId;

    public CharacterController(int playerId)
    {
        PlayerId = playerId;
    }

    public bool Equals(CharacterController other)
    {
        return PlayerId == other.PlayerId;
    }
}
```
Keys are structs inheriting `IEntityIndexKey<TSelf>`. And you have to implement `bool Equals(TSelf)` to check Key equality and optionally implement `int GetHashCode()`. Also keys are not meant to be mutable so I prefer to add `readonly` constraint.

Then, you add `Indexed<TKey>` to your descriptor.

```csharp
public class CharacterDescriptor<HealthComponent, PositionComponent, Indexed<CharacterController>> { }
```
Indexed is special component to make sure that indexes are up-to-date. It has Controller struct as member `Key`, but you cannot change the value unless you use `Indexed<TKey>.Update(SchemaContext, TKey)`. `SchemaContext` is returned when `AddSchema<TSchema>()` is executed and represents runtime state of schema.

Before look into `SchemaContext`, Let's add `Index<TKey>` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    private static Table<CharacterDescriptor> _flyingCharacter = new Table<CharacterDescriptor>();
    public static Group<CharacterDescriptor> FlyingCharacter => _flyingCharacter.Group();

    private static Table<CharacterDescriptor> _groundCharacter = new Table<CharacterDescriptor>();
    public static Group<CharacterDescriptor> GroundCharacter => _flyingCharacter.Group();

    private static Index<CharacterController> _charactersByController = new Index<CharacterController>();
    public static IndexQuery<ChracterController> CharactersByController(int playerId) => _charactersByController.Query(new CharacterController(playerId));
}
```
`Index<TKey>` will index any `Indexed<TKey>` component in any tables with same partition. Any child partition will be indexed as well. If `Index<TKey>` is defined in root schema, any table with `Indexed<TKey>` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` group will be indexed and returned when queried. If you want to index specific groups only, define a partition.

Same manner as we expose a group for a table, we'll expose `IndexQuery<TKey>` for a index. `IndexQuery<TKey>` is query for a specific key, like 'entities controlled by player id 0'.

You can share `Indexed<TKey>` across different descriptors.

### Querying Indexes
Now, finally you can iterate over entities with `IndexQuery<TKey>`. You don't have to include `Indexed<TKey>` in the type list. You can query any type of component within the descriptor, because as long as you keep a group with single descriptor you can iterate with same filter.

Just like when you query with `EntitiesDB`, you query with `SchemaContext`.
```csharp
foreach (var ((health, position, indices), group) in IndexedSchema.CharactersByController(3).Entities<HealthComponent, PositionComponent>(schemaContext))
{
    for (int i = 0; i < indices.count(); ++i)
    {
        health[indices[i]].current += 10;
    }
}
```
Note that you have to use double indexing like `health[indices[i]]`.  **DO NOT update `Indexed` component while iterating through index query with it.** It is undefined behaviour.

If you want to query index within specific `Group<T>` or `Groups<T>`, use `From` like this:
```csharp
var (health, position, indices) = IndexedSchema.CharactersByController(3)
    .From(IndexedSchema.FlyingCharacter)
    .Entities<HealthComponent, PositionComponent>(schemaContext);
```

## Naming Convention
Below is naming convention suggestions to make schema more readable.

### For Tables and Partitions
* Use `_singularNoun` for singluar table.
* Use `_pluralNouns` for ranged table.
* Use `SingularNoun` for `Group<T>`.
* Use `PluralNouns` for `Groups<T>`.

### For Partitions
* Use `_adjective` or `_singluarNoun` for singular partition. e.g. `_flying`
* Use `_adjective` or `_pluralNouns` for ranged partition.
* Use `Adjective` or `SingularNoun` for result of `Shard()`. e.g. `Flying`, so you can access like `Flying.Monster`
* Use `Adjective` or `PluralNouns` for result of `Shards()`.

### For Indexes
* Use `TableNameKeyName` for `IEntityIndexKey`. e.g. `ItemHolder`
* Use `_tableNamesByKeyName` for `Index<T>`. e.g. `_itemsByHodler`
* Use `TableNameByKeyName` for `IndexQuery`. e.g. `ItemsByHolder`
