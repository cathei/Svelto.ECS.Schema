## Using Schemas
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

### Using Schema
Now we defined a schema, we can add it to `EnginesRoot`, do this before any entitiy submission.

```csharp
IndexedDB indexedDB = _enginesRoot.GenerateIndexedDB();
GameSchema schema = _enginesRoot.AddSchema<GameSchema>(indexedDB);
```
Generating `IndexedDB` is required prior to generate schema. `IndexedDB` holds `EntitiesDB` and indexing information of entities. You can use `IndexedDB` anywhere you need `EntitiesDB`. I recommend you to just inject `IndexedDB` to your Engines instead of inheriting `IQueryingEntitiesEngine` when you using Schema extensions.

And call `AddSchema` for `GameSchema`, you can access to your Schema with return value. Make sure you use Schema object returned by `AddSchema`. In other words do NOT call new on root Schema.

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
var (egid, count) = schema.Character.Entities<EGIDComponent>(indexedDB);
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
foreach (var ((egid, count), group) in schema.Players.Entities<EGIDComponent>(indexedDB)) { }
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
        var indexedDB = _enginesRoot.GeneratedIndexedDB();
        var schema = _enginesRoot.AddSchema<GameSchema>(indexedDB);

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
foreach (var ((healths, positions, count), group) in schema.AllAliveCharacters.Entities<HealthComponent, PositionComponent>(indexedDB))
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
