[![Nuget](https://img.shields.io/nuget/v/Svelto.ECS.Schema)](https://www.nuget.org/packages/Svelto.ECS.Schema/) [![GitHub](https://img.shields.io/github/license/cathei/Svelto.ECS.Schema)](https://github.com/cathei/Svelto.ECS.Schema/blob/master/LICENSE) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/Dvak3QMj3n)

## Svelto.ECS.Schema
Extension for [Svelto.ECS](https://github.com/sebas77/Svelto.ECS), helps defining structure like database schema.

### Motivation
Svelto.ECS is an awesome project, however I found understanding underlying entity structure can be pretty confusing to new users like myself.
It has powerful tools like groups and filters, but lacks of wrapper to make it intutive.
I thought it will be much easier to understand group with structured schema, and it is worth to make your code flexible, design change proof.
That is the motivation I wrote this Svelto.ECS.Schema extension which is basically a user-friendly wrapper for groups and filters.

### Concept
Think of a RDBMS schema, there is tables, records, columns, indexes and shards. ECS is basically in-memory database but faster.
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
    private Table<CharacterDescriptor> _character = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> Character => _character.Group();

    private Table<ItemDescriptor> _item = new Table<ItemDescriptor>();
    public Group<ItemDescriptor> Item => _item.Group();
}
```
`IEntitySchema` is a logical group that can contain tables and indexes as member.

`Table<TDescriptor>` represents underlying `ExclusiveGroup`. Groups should only accept entities using same descriptor, or else the iteration index will break. In Schema extension Table has Descriptor type argument, basically preventing the issue.

Note that tables are defined as private, and only `Group<T>` are exposed. This is pattern I recommend, so rest of your code can be kept clean and reslove all schema definition related code in schema classes.

### Generating Schema
Now we defined a schema, we can generate on through `EnginesRoot`, do this before any entitiy submission.

```csharp
IndexesDB indexesDB = _enginesRoot.GenerateIndexesDB();
GameSchema schema = _enginesRoot.GenerateSchema<GameSchema>(indexesDB);
```
Generating `IndexesDB` is required prior to generate schema. It is the class that will hold indexing information of a `EnginesRoot`. We will use this later.

**Do not call new on IEntityScheam directly. It needs to be managed internally by this extension.**

### Add Entities to Table
Now to add entity with `Group<T>`, we support two ways. One is original Svelto way:
```csharp
var entityBuilder = entityFactory.BuildEntity(entityId, schema.Character);
```
The other is Schema extension way (preferred):
```csharp
var entityBuilder = schema.Character.Build(entityFactory, entityId);
```
Results are the same so it is just different expression.

### Query Entities from Table
To query entities of `Group<T>`, also original Svelto way:
```csharp
var (egid, count) = entitiesDB.QueryEntities<EGIDComponent>(schema.Character);
```
and Schema extension way (preferred):
```csharp
var (egid, count) = schema.Character.Entities<EGIDComponent>(entitiesDB);
```

Reason I decided to have different expression from original Svelto is because we can pass more type information to calls without making it look awkward. C# does not support partial generic type inference, and we don't wanna verbosely pass type information around like `QueryEntities<EGIDCompoent, CharacterDescriptor>(GameSchema.Character)`.

### Defining Ranged Table
Sometimes you'll want many tables of same type, without defining many variables. Simiply pass the number of group you want to be created, and there are multiple separated tables!
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class AnotherSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    private Table<ItemDescriptor> _items = new Table<ItemDescriptor>((int)ItemType.MAX);
    public Group<ItemDescriptor> Items(ItemType type) => _items.Group((int)type);

    private Table<PlayerDescriptor> _players = new Table<PlayerDescriptor>(MaxPlayerCount);
    public Group<PlayerDescriptor> Player(int playerId) => _players.Group(playerId);

    public Groups<PlayerDescriptor> AllPlayers { get; }

    public AnotherSchema()
    {
        AllPlayers = _players.Groups();
    }
}
```
Above example shows use case of ranged tables with number or enum.

Note that we also exposes `AnotherSchema.AllPlayers` which represents all player groups. `Groups<T>` has underlying `FasterList<ExclusiveGroupStruct>`. Which means you can directly pass it into `EntitiesDB.QueryEntities`. 
```csharp
foreach (var (...) in entitiesDB.QueryEntities<...>(schema.AllPlayers)) { }
```
Or, in Schema extension way (preferred):
```csharp
foreach (var (...) in schema.AllPlayers.Entities<...>(entitiesDB)) { }
```

### Defining Shards
On the other hand, you will want to make separate group for some related tables, and reuse it. We use `Shard<TSchema>` for it. First, define a child schema, same as we defined other schemas before.
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class PlayerSchema : IEntitySchema
{
    private Table<CharacterDescriptor> _aliveCharacter = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> AliveCharacter => _aliveCharacter.Group();

    private Table<CharacterDescriptor> _deadCharacter = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> DeadCharacter => _deadCharacter.Group();

    private Table<ItemDescriptor> _items = new Table<ItemDescriptor>((int)ItemType.MAX);
    public Group<ItemDescriptor> Item(ItemType type) => _items.Group((int)type);
}
```
Now we have `PlayerSchema`, we can define shard in the parent schema. with `Shard<PlayerSchema>`.

```csharp
public class MyGameSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    private Shard<PlayerSchema> _ai = new Shard<PlayerSchema>();
    public PlayerSchema AI => _ai.Schema();

    private Shard<PlayerSchema> _players = new Shard<PlayerSchema>(MaxPlayerCount);
    public PlayerSchema Player(int playerId) => _players.Schema(playerId);

    public Groups<CharacterDescriptor> AllAliveCharacters { get; }

    public MyGameSchema()
    {
        AllAliveCharacters = AI.AliveCharacter + _players.Schemas().Combine(x => x.AliveCharacter);
    }
}
```
Nice. As you can see, shard can be ranged, too. We defined a group for AI, and 10 players. Just like how we expose group instead of table, we'll expose inner schema insted of shard itself. But again aware that no schema should created directly. If you want to access group for player 5's alive characters, use `MyGameSchema.Player(5).AliveCharacter`. Also we added shortcut `Groups` for all alive characters.

Let's see how to fill up your tables with records.
```csharp
public class CompositionRoot
{
    private uint eidCounter = 0;

    public CompositionRoot()
    {
        var submissionScheduler = new SimpleEntitiesSubmissionScheduler();
        var enginesRoot = new EnginesRoot(submissionScheduler);

        var entityFactory = enginesRoot.GenerateEntityFactory();
        var indexesDB = _enginesRoot.GenerateIndexesDB();
        var schema = _enginesRoot.GenerateSchema<GameSchema>(indexesDB);

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, schema.AI.AliveCharacter);

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, schema.Player(0).DeadCharacter);

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
Above we have example to put 10 characters to alive, AI controlled character group, and put another 10 characters to dead, player 0 controlled character group. Now you can inject schema to your preferred engine and query entities. You don't have to specify descriptor when build entity, because group is already implying descriptor type.
```csharp
foreach (var ((healths, positions, count), group) in schema.AllAliveCharacters.Entities<HealthComponent, PositionComponent>(entitiesDB))
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
public class StateSchema : IEntitySchema
{
    private Table<DoofusEntityDescriptor> _doofus = new Table<DoofusEntityDescriptor>();
    public Group<DoofusEntityDescriptor> Doofus => _doofus.Group();

    private Table<FoodEntityDescriptor> _food = new Table<FoodEntityDescriptor>();
    public Group<FoodEntityDescriptor> Food => _food.Group();
}

public class TeamSchema : IEntitySchema
{
    private Shard<StateSchema> _eating = new Shard<StateSchema>();
    public StateSchema Eating => _eating.Schema();

    private Shard<StateSchema> _nonEating = new Shard<StateSchema>();
    public StateSchema NonEating => _nonEating.Schema();
}

public enum TeamColor { Red, Blue, MAX }

public class GameSchema : IEntitySchema
{
    private Shard<TeamSchema> _team = new Shard<TeamSchema>((int)TeamColor.MAX);
    public TeamSchema Team(TeamColor color) => _team.Schema((int)color);

    public Groups<DoofusEntityDescriptor> EatingDoofuses { get; }

    public GameSchema()
    {
        EatingDoofuses = _team.Schemas().Combine(x => x.Eating.Doofus);
    }
}
```
More code, but you'll thank to some complexity when you have to deal with big design changes!

When using it, code `GameGroups.RED_DOOFUSES_EATING.Groups` would be equvalent to `GameSchema.Team(TeamColor.Red).Eating.Doofus`.

## Index Usage
### Defining Indexes
Index is wrapper of filters system, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a shard or entire schema. Let's take a look. You have to define Key first.
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
Indexed is special component to make sure that indexes are up-to-date. It has Controller struct as member `Key`, but you cannot change the value unless you use `Indexed<TKey>.Update(IndexesDB, TKey)`. `IndexesDB` is returned when `EnginesRoot.GenerateIndexesDB()` is executed and represents runtime state of entity indexes.

Before look how to query with indexes, Let's add `Index<TKey>` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    private Table<CharacterDescriptor> _flyingCharacter = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> FlyingCharacter => _flyingCharacter.Group();

    private Table<CharacterDescriptor> _groundCharacter = new Table<CharacterDescriptor>();
    public Group<CharacterDescriptor> GroundCharacter => _flyingCharacter.Group();

    private Index<CharacterController> _charactersByController = new Index<CharacterController>();
    public IndexQuery<ChracterController> CharactersByController(int playerId) => _charactersByController.Query(new CharacterController(playerId));
}
```
`Index<TKey>` will index any `Indexed<TKey>` component in any tables with same schema. Any child schema will be indexed as well. If `Index<TKey>` is defined in root schema, any table with `Indexed<TKey>` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` group will be indexed and returned when queried. If you want to index specific groups only, define a shard.

Same manner as we expose a group for a table, we'll expose `IndexQuery<TKey>` for a index. `IndexQuery<TKey>` is query for a specific key, like 'entities controlled by player id 0'.

You can share `Indexed<TKey>` across different descriptors.

### Querying Indexes
Now, finally you can iterate over entities with `IndexQuery<TKey>`. You don't have to include `Indexed<TKey>` in the type list. You can query any type of component within the descriptor, because as long as you keep a group with single descriptor you can iterate with same filter.

Just like when you query with `EntitiesDB`, you query with `IndexesDB`.
```csharp
foreach (var ((health, position, indices), group) in schema.CharactersByController(3).Entities<HealthComponent, PositionComponent>(indexesDB))
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
    .Entities<HealthComponent, PositionComponent>(indexesDB);
```

## Advanced Usage
### Extending Schema
In advance, you can extend your schema with inheritance, or having multiple schemas in same `EnginesRoot`. You can still share `IndexesDB` between schemas. Good thing is, underlying groups will remain static and unique per schema type.

```csharp
public abstract class GameModeSchemaBase : IEntitySchema
{
    protected Shard<PlayerSchema> _players;
    public PlayerSchema Player(int playerId) => _players.Schema(playerId);

    public GameModeSchemaBase(int playerCount)
    {
        _players = new Shard<PlayerSchema>(playerCount);
    }
}

public class PvPGameModeSchema : GameModeSchemaBase
{
    // eight player max
    public PvPGameModeSchema() : base(8) { }
}

public class CoOpGameModeSchema : GameModeSchemaBase
{
    protected Shard<PlayerSchema> _ai;
    public PlayerSchema AI => _ai.Schema();

    // two player max
    public CoOpGameModeSchema() : base(2) { }
}

```

## Naming Convention
Below is naming convention suggestions to make schema more readable.

### For Tables
* Use `_singularNoun` for singluar table.
* Use `_pluralNouns` for ranged table.
* Use `SingularNoun` for `Group<T>`.
* Use `PluralNouns` for `Groups<T>`.

### For Shards
* Use `_adjective` or `_singluarNoun` for singular shard. e.g. `_flying`
* Use `_adjective` or `_pluralNouns` for ranged shard.
* Use `Adjective` or `SingularNoun` for result of `Schema()`. e.g. `Flying`, so you can access like `Flying.Monster`
* Use `Adjective` or `PluralNouns` for result of `Schema()`.

### For Indexes
* Use `TableNameKeyName` for `IEntityIndexKey`. e.g. `ItemHolder`
* Use `_tableNamesByKeyName` for `Index<T>`. e.g. `_itemsByHodler`
* Use `TableNameByKeyName` for `IndexQuery`. e.g. `ItemsByHolder`

### Etc.
* Use `Indexes` as plural form for `Index` in schema.
* Use `indices` as plural form for index of array.
