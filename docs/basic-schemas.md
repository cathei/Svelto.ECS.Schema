## Using Schemas
Using Schema, we can define hierarchical structure of Entities in our game.

### Defining Schema
Let's define simplest Schema. You'll need to define Rows first.
```csharp
public struct DamagableSet : IResultSet<HealthComponent, DefenseComponent>
{
    public NB<HealthComponent> health;
    public NB<DefenseComponent> defense;
    public int count { get; set; }

    public void Init(EntityCollection<HealthComponent, DefenseComponent> collection)
        => (health, defense, count) = collection;
}

public struct StackableSet : IResultSet<AmountComponent>
{
    public NB<AmountComponent> stack;
    public int count { get; set; }

    public void Init(EntityCollection<AmountComponent> collection)
        => (stack, count) = collection;
}

public sealed class CharacterRow : DescriptorRow<CharacterRow>, IQueryable<DamagableSet> {}
public sealed class ItemRow : DescriptorRow<ItemRow>, IQueryable<StackableSet> {}
```
And put the Tables in your Schema.
```csharp
public class GameSchema : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new Table<CharacterRow>();
    public readonly Table<ItemRow> Item = new Table<ItemRow>();
}
```
`IEntitySchema` is a logical group that can contain Tables and Indexes as their members. I strongly recommend to make every fields in Schema `readonly`. Their values are not meant to be changed.

`Table<DescriptorRow>` is a Table that can hold specific Descriptor Row. Since it is exlusive, a Entity will belong in one Group only at the same moment. In Svelto Groups should accept entities using same Descriptor, or else the iteration index will break or mixed. In Schema extension Table tied with Descriptor Row, basically preventing this issue.

### Using Schema
Now we defined a schema, we can add it to `EnginesRoot`, do this before any entitiy submission.

```csharp
IndexedDB indexedDB = _enginesRoot.GenerateIndexedDB();
GameSchema schema = _enginesRoot.AddSchema<GameSchema>(indexedDB);
```
Generating `IndexedDB` is required prior to generate schema. `IndexedDB` replaces `EntitiesDB`, and works as core of Schema extensions. I recommend you to just inject `IndexedDB` to your Engines instead of inheriting `IQueryingEntitiesEngine` when you using Schema extensions, since you can query all the Entities through `IndexedDB`.

And call `AddSchema` for `GameSchema`, you can access to your Schema with return value. Make sure you use Schema object returned by `AddSchema`. In other words do **NOT** call `new` on root Schema.

### Add Entities to Table
Now to add entity to a `Table`, you can call `IEntityFactory.Build` extension method.
```csharp
var builder = entityFactory.Build(schema.Character, entityId);
builder.Init(new HealthComponent(100));
builder.Init(new DefenseComponent(5));
```
Note that it automatically uses Descriptor from Descriptor Row, compare to original `BuildEntity`. We will get to how to query entities in next article.

### Remove Entities or Move Entities between Tables
You can call `IEntityFunctions.Remove(Table, entityID)` to remove Entity from Table. You can call `IEntityFunctions.Move(FromTable, entityID).To(ToTable)` to move Entity between Tables.

### Defining Ranged Table
Sometimes you'll want many tables of same type, without defining many variables. Simiply use `Tables<DescriptorRow>` (aware of 's' in the end), pass the number of group you want to be created, and there are multiple separated tables!
```csharp
public enum ItemType { Potion, Weapon, Armor, MAX };

public class AnotherSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    public readonly Tables<PlayerRow> Players = new Tables<PlayerRow>(MaxPlayerCount);

    public readonly Tables<ItemRow, ItemType> Items = new Tables<ItemRow><ItemType>(ItemType.MAX, type => (int)type);
}
```
Above example shows use case of `Tables` with `int` or `enum`. `Players` has one argument since it is using integer, and `Items` has a mapping function to access inner table easily. Both tables are accessable by `Players[0]` or `Items[0]`. Additionally, item tables are accessible with `ItemType` like `Items[ItemType.Potion]`.

### Defining Nested Schemas
On the other hand, you will want to make separate group for some related tables, and reuse it. First, define a child schema, same as we defined other schemas before.
```csharp
public class PlayerSchema : IEntitySchema
{
    public readonly Table<CharacterRow> AliveCharacter = new Table<CharacterRow>();
    public readonly Table<CharacterRow> DeadCharacter = new Table<CharacterRow>();

    public readonly Tables<ItemRow, ItemType> Items = new Tables<ItemRow, ItemType>(ItemType.MAX, type => (int)type);
}

public enum ItemType { Potion, Weapon, Armor, MAX };
```
Now we have `PlayerSchema`, we can now add child Schema in the parent Schema. Even more, we can define multiple child Schemas with `Ranged<TSchema>`.

```csharp
public class MyGameSchema : IEntitySchema
{
    public const int MaxPlayerCount = 10;

    public readonly PlayerSchema AI = new PlayerSchema();

    public readonly Ranged<PlayerSchema> Players = new Ranged<PlayerSchema>(MaxPlayerCount);

    public readonly CombinedTables<CharacterRow> AllAliveCharacters;

    public MyGameSchema()
    {
        AllAliveCharacters = AI.AliveCharacter.Append(Players.Combine(x => x.AliveCharacter));
    }
}
```
Nice. We defined a child Schema for AI, and 10 child Schemas for players. If you want to access group for player 5's alive characters, use `MyGameSchema.Player[5].AliveCharacter`.

Also note that we added shortcut `CombinedTables<T>` to combine all alive characters from AI and all players. You can use it same as `Tables<DescriptorRow>`. You can also use this type to combine Tables share same Interface Rows. For example, you could define `CombindedTables<IStackableRow>`.

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

    private void AddCharacter(IEntityFactory entityFactory, Table<CharacterRow> table)
    {
        var builder = entityFactory.Build(table, eidCounter++);

        builder.Init(new HealthComponent(1000));
        builder.Init(new DefenseComponent(5));
    }
}
```
Above we have example to put 10 characters to alive, AI controlled character group, and put another 10 characters to dead, player 0 controlled character group.

### Summary
We looked into how to define Tables and Schemas. In [Next Document](basic-queries.md), it is time to perform query to iterate Entities we built.