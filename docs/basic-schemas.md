## Using Schemas
Using Schema, we can define hierarchical structure of Entities in our game.

### Defining Schema
Let's define simplest Schema. You'll need to define Rows first.
```csharp
public struct DamagableSet : IResultSet<HealthComponent, DefenseComponent>
{
    public NB<HealthComponent> health;
    public NB<DefenseComponent> defense;

    public void Init(EntityCollection<HealthComponent, DefenseComponent> collection)
        => (health, defense, _) = collection;
}

public struct StackableSet : IResultSet<AmountComponent>
{
    public NB<AmountComponent> stack;

    public void Init(EntityCollection<AmountComponent> collection)
        => (stack, _) = collection;
}

public sealed class CharacterRow : DescriptorRow<CharacterRow>, IQueryable<DamagableSet> {}
public sealed class ItemRow : DescriptorRow<ItemRow>, IQueryable<StackableSet> {}
```
And put the Tables in your Schema.
```csharp
public class GameSchema : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new();
    public readonly Table<ItemRow> Item = new();
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

### Remove Entities from Table
You can call `IndexedDB.Remove(EGID)` to remove Entity from Table.

### Move Entities between Tables
You can call `IndexedDB.Move(EGID, ToTable)` to move Entity between Tables.

### Summary
We looked into how to define Tables and Schemas. In [Next Document](basic-queries.md), it is time to perform query to iterate Entities we built.