[![Nuget](https://img.shields.io/nuget/v/Svelto.ECS.Schema)](https://www.nuget.org/packages/Svelto.ECS.Schema/) [![GitHub](https://img.shields.io/github/license/cathei/Svelto.ECS.Schema)](https://github.com/cathei/Svelto.ECS.Schema/blob/master/LICENSE) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/Dvak3QMj3n)

## Svelto.ECS.Schema
Extension for [Svelto.ECS](https://github.com/sebas77/Svelto.ECS), helps defining structure like database schema.

### Motivation
Svelto.ECS is an awesome project, however I found understanding underlying entity structure can be pretty confusing to new users like myself.
It has powerful tools like groups and filters, but lacks of wrapper to make it intutive.
I thought it will be much easier to understand group with structured Schema, and it is worth to make your code flexible, design change proof.
That is the motivation I wrote this Svelto.ECS.Schema extension which is basically a user-friendly wrapper for groups and filters.

Think of a RDBMS schema, there is tables, records, columns, indexes. ECS is basically in-memory database but faster.
In RDBMS, tables can hold records having specific combination of columns.
In Svelto.ECS, groups can hold entities having specific combination of components.
That is why I chose to take friendly terms from RDBMS and define Schema of ECS.

### Features
* RDBMS-like Schema Definition with Extendible, Nestable Layout 
* Indexing Entities and Automatic Tracking over Tables
* Finite State Machine supporting Transitions, Conditions and Callbacks

## Basic Usage
### Install
Currently it is alpha stage, available on [NuGet](https://www.nuget.org/packages/Svelto.ECS.Schema/). While I don't recommend to use it on production, feel free to try it and please share me the experience!

### Need help?
If you need help or want to give feedback, you can either join [my Discord Channel](https://discord.gg/Dvak3QMj3n) or ping @cathei from [Svelto's Official Discord Channel](https://discord.gg/3qAdjDb).

### Defining Descriptor
Let's say you have basic understanding of Svelto ECS. (You might read some articles and still confused and lost.)
Defining Schema starts from defining EntityDescriptor, that is combination of components.
```csharp
public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent, HealthComponent, PositionComponent> { }
```
It is not part of this extension, but it is important because it is basically definition of records that table can hold.

### Defining Schema
Let's define simplest Schema.
```csharp
public class GameSchema : IEntitySchema
{
    public readonly Table<CharacterDescriptor> Character = new Table<CharacterDescriptor>();
    public readonly Table<ItemDescriptor> Item = new Table<ItemDescriptor>();
}
```
`IEntitySchema` is a logical group that can contain Tables and Indexes as their members. I strongly recommend to make every fields in Schema `readonly`.

`Table<TDescriptor>` represents underlying `ExclusiveGroup`. Since it is exlusive, a Entity will belong in one Group only at the same moment. Groups should only accept entities using same Descriptor, or else the iteration index will break. In Schema extension Table has Descriptor type argument, basically preventing this issue.

Note that tables are public readonly fields. Tables should not be changed, and properties are not supported by Schema extension for now.

### Generating Schema
Now we defined a schema, we can generate on through `EnginesRoot`, do this before any entitiy submission.

```csharp
IndexesDB indexesDB = _enginesRoot.GenerateIndexesDB();
GameSchema schema = _enginesRoot.AddSchema<GameSchema>(indexesDB);
```
Generating `IndexesDB` is required prior to generate schema. It is the class that will hold runtime information for Schema extension. We will use this later. Make sure you use Schema object returned by `AddSchema`. In other words do NOT call new on root Schema.

### Add Entities to Table
Now to add entity with `Table<T>`, we support two ways. You can pass Table to `BuildEntity` as if it is a `ExclusiveGroup`;
```csharp
var entityBuilder = entityFactory.BuildEntity(entityId, schema.Character);
```
Or call through `Table<T>.Build`.
```csharp
var entityBuilder = schema.Character.Build(entityFactory, entityId);
```
Results are the same so it is just different expression. But later gives us more type information, so later is preffered. From now we will introduce more with form of later, but keep in mind that many of them can be also used with equivalent expression like former.

### Query Entities from Table
To query entities of `Table<T>`, it is easy as building entity.
```csharp
var (egid, count) = schema.Character.Entities<EGIDComponent>(entitiesDB);
```

### Defining Ranged Table
Sometimes you'll want many tables of same type, without defining many variables. Simiply define `Tables<T>`, pass the number of group you want to be created, and there are multiple separated tables!
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class AnotherSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    public readonly Tables<PlayerDescriptor> Players = new Tables<PlayerDescriptor>(MaxPlayerCount);

    public readonly Tables<ItemDescriptor, ItemType> Items
        = new Tables<ItemDescriptor, ItemType>((int)ItemType.MAX, itemType => (int)itemType);
}
```
Above example shows use case of `Tables` with `int` or `enum`. `Players` has one argument since it is using integer, and `Items` has a mapping function to access inner table easily. Both tables are accessable by `Players[0]` or `Items[0]`. Additionally, item tables are accessible with `ItemType` like `Items[ItemType.Potion]`.

`Tables<T>` has underlying `FasterList<ExclusiveGroupStruct>`. Which means you can query over multiple groups with it:
```csharp
foreach (var ((egid, count), group) in schema.Players.Entities<EGIDComponent>(entitiesDB)) { }
```

### Defining Nested Schemas
On the other hand, you will want to make separate group for some related tables, and reuse it. First, define a child schema, same as we defined other schemas before.
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class PlayerSchema : IEntitySchema
{
    public readonly Table<CharacterDescriptor> AliveCharacter = new Table<CharacterDescriptor>();
    public readonly Table<CharacterDescriptor> DeadCharacter = new Table<CharacterDescriptor>();

    public readonly Tables<ItemDescriptor, ItemType> Items =
        new Tables<ItemDescriptor, ItemType>((int)ItemType.MAX, itemType => (int)itemType);
}
```
Now we have `PlayerSchema`, we can now add child Schema in the parent Schema. Even more, we can define multiple child Schemas with `Ranged<TSchema>`.

```csharp
public class MyGameSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    public readonly PlayerSchema AI = new PlayerSchema();

    public readonly Ranged<PlayerSchema> Players = new Ranged<PlayerSchema>(MaxPlayerCount);

    public readonly Tables<CharacterDescriptor> AllAliveCharacters;

    public MyGameSchema()
    {
        AllAliveCharacters = AI.AliveCharacter + Players.Combine(x => x.AliveCharacter);
    }
}
```
Nice. We defined a child Schema for AI, and 10 child Schemas for players. If you want to access group for player 5's alive characters, use `MyGameSchema.Player[5].AliveCharacter`.

Also note that we added shortcut `Tables` for all alive characters of AI and all players. You can use it same as other `Tables` you defined directly.

Let's see complete example to fill up your tables with records.
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
        var schema = _enginesRoot.AddSchema<GameSchema>(indexesDB);

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, schema.AI.AliveCharacter);

        for (int i = 0; i < 10; ++i)
            AddCharacter(entityFactory, schema.Player[0].DeadCharacter);

        submissionScheduler.SubmitEntities();
    }

    private void AddCharacter(IEntityFactory entityFactory, Table<CharacterDescriptor> table)
    {
        var builder = table.Build(entityFactory, eidCounter++);

        builder.Init(new HealthComponent(1000));
        builder.Init(new PositionComponent(0, 0));
    }
}
```
Above we have example to put 10 characters to alive, AI controlled character group, and put another 10 characters to dead, player 0 controlled character group. Now you can inject schema to your preferred engine and query entities. You don't have to specify descriptor when build, swap or remove entity, because group is already implying descriptor type.
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
    public readonly Table<DoofusEntityDescriptor> Doofus = new Table<DoofusEntityDescriptor>();
    public readonly Table<FoodEntityDescriptor> Food = new Table<FoodEntityDescriptor>();
}

public class TeamSchema : IEntitySchema
{
    public readonly StateSchema Eating = new StateSchema();
    public readonly StateSchema NonEating = new StateSchema();
}

public class GameSchema : IEntitySchema
{
    public readonly Ranged<TeamSchema, TeamColor> Teams =
        new Ranged<TeamSchema, TeamColor>((int)TeamColor.MAX, teamColor => (int)teamColor);

    public readonly Tables<DoofusEntityDescriptor> EatingDoofuses;

    public GameSchema()
    {
        EatingDoofuses = Teams.Combine(x => x.Eating.Doofus);
    }
}

public enum TeamColor { Red, Blue, MAX }
```
Now we can easly change structure without fixed names, and have changable number of teams. You'll thank to some complexity when you have to deal with big design changes!

When using it, code `GameGroups.RED_DOOFUSES_EATING.Groups` would be equvalent to `GameSchema.Teams[TeamColor.Red].Eating.Doofus`.

## Index Usage
### Defining Indexes
Index is wrapper of filters system, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a child or entire schema. Let's take a look. We'll start from defining `IndexTag`.
```csharp
public class CharacterController : IndexTag<int, CharacterController.Unique>
{
    public struct Unique : IUnique { }
}
```
IndexTag represent a indexable trait of entity. First type parameter is equatable value type that will be used as key of Index. Second type parameter is to ensure uniqueness of the generic class members (We have to define struct to pass as type parameter, due to Svelto limitation).

`IndexTag` has nested types of `Component` and `Index`. Now, let's add `CharacterController.Component` to your descriptor.
```csharp
public class CharacterDescriptor<HealthComponent, PositionComponent, CharacterController.Component> { }
```
`IndexTag.Component` is a special component holds the `Value` to index, and ensures that indexes are up-to-date. It has the first type parameter of `IndexTag`, which is `int` here, as member `Value`, but you cannot change the `Value` directly. Instead you need to call `Update(IndexesDB, TValue)`. `IndexesDB` is returned when `EnginesRoot.GenerateIndexesDB()`, holds runtime state of entity indexes.

Before look how to query with indexes, Let's add `CharacterController.Index` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    public readonly Table<CharacterDescriptor> FlyingCharacter = new Table<CharacterDescriptor>();
    public readonly Table<CharacterDescriptor> GroundCharacter = new Table<CharacterDescriptor>();

    public readonly CharacterController.Index CharactersByController = new CharacterController.Index();
}
```
`IndexTag.Index` will index paired `IndexTag.Component` component in any tables within declared schema. Any child schema will be indexed as well. Since `CharacterController.Index` is defined in root schema, any table with `CharacterController.Component` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` group will be indexed and returned when queried. If you want to index specific groups only, define a child Schema.

Also, you can share `IndexTag.Component` across different descriptors. Index will handle them well.

### Querying Indexes
Now, finally you can iterate over entities with `IndexTag.Index`. You don't have to include `IndexTag.Component` in the type list. You can query any type of component within the descriptor, because as long as you keep a group with single descriptor you can iterate with same filter.

Just like when you query with `EntitiesDB`, you query with `IndexesDB`. To query entites with `IndexTag.Component.Value` of 3:
```csharp
foreach (var ((health, position, indices), group) in schema.CharactersByController
    .Query(3).Entities<HealthComponent, PositionComponent>(indexesDB))
{
    foreach (var i in indices)
    {
        health[i].current += 10;
    }
}
```
Note that you can use foreach loop to iterate indices.  but **DO NOT** update `IndexTag.Component` component while iterating through index query with it. It is undefined behaviour. If you have to, consider using `StateMachine` instead, which will be explained later.

If you want to query index within specific `Table<T>` or `Tables<T>`, use `From` like this:
```csharp
var (health, position, indices) = schema.CharactersByController
    .Query(3).From(schema.FlyingCharacter).Entities<HealthComponent, PositionComponent>(indexesDB);
```

## State Machine Usage
### Defining State Machine

### Adding Transitions

### Adding Callbacks

### Adding State Machine

### 

## Advanced Usage
### Extending Schema
In advance, you can extend your Schema with inheritance, or having multiple Schemas within same `EnginesRoot`. You can still share `IndexesDB` between schemas. Good thing is, underlying groups will remain static and unique per added Schema type.

```csharp
public abstract class GameModeSchemaBase : IEntitySchema
{
    public readonly Ranged<PlayerSchema> Players;

    public GameModeSchemaBase(int playerCount)
    {
        _players = new Ranged<PlayerSchema>(playerCount);
    }
}

public class PvPGameModeSchema : GameModeSchemaBase
{
    // eight player max
    public PvPGameModeSchema() : base(8) { }
}

public class CoOpGameModeSchema : GameModeSchemaBase
{
    public PlayerSchema AI = new PlayerSchema();

    // two player max
    public CoOpGameModeSchema() : base(2) { }
}
```

### Calculate Union and Intersection of Indexes
To calculate Union and Intersection of Indexes, you can use temporary filters called `Memo<T>`. It can be included anywhere in Schema. Use it like this:
```csharp
_schema.CharacterByController.Query(0).Union(_indexesDB, _schema.Memo);
_schema.CharacterByController.Query(3).Union(_indexesDB, _schema.Memo);
_schema.CharacterByController.Query(6).Union(_indexesDB, _schema.Memo);

_schema.CharacterByState.Query(CharacterState.Happy).Intersect(_indexesDB, _schema.Memo);
```
Note that you have to clear `Memo<T>` before you reuse it! `Memo<T>` does not have any guarantee to have valid indices after entity submission.

## Naming Convention
Below is naming convention suggestions to make schema more readable.

### For Tables
* Use `SingularNoun` for `Table<T>`.
* Use `PluralNouns` for `Tables<T>`.

### For Schemas
* Use `Adjective` or `SingularNoun` for Schema object. e.g. `Flying`, so you can access like `Flying.Monster`
* Use `Adjective` or `PluralNouns` for `Ranged<TSchema>`

### For Indexes
* Use `TableNameKeyName` for `IndexTag` type. e.g. `ItemHolder`
* Use `TableNameByKeyName` for `IndexTag.Index`. e.g. `ItemsByHolder`

### Etc.
* Use `Indexes` as plural form for `Index` in schema.
* Use `indices` as plural form for index of array.
