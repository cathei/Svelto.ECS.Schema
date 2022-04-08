## Using Primary Keys
Using `Primary Key`, we can define separated `Group` memory structure of Entities. When you use `Primary Key`, you should think it as part of RDBMS primary key, that you can query partially. For example if you have primary key like this,
```sql
CREATE TABLE Character(
    EntityID int NOT NULL,
    PlayerID int NOT NULL,
    CONSTRAINT PK_Character PRIMARY KEY (PlayerID, EntityID)
);
```
Then you can query all rows with specific `PlayerID` with best performance. That is what happening in Schema extension as well. Some difference is that you can make any partial queries no matter how the column order is! 

### Defining Key Component
First we need to define `IKeyComponent<TKey>`. It is special `IEntityComponent` that holds the key value for `Primary Key` or `Index`. It only requires `TKey key { get; set; }` property.
```csharp
public struct IntKeyComponent : IKeyComponent<int>
{
    public int key { get; set; }
}
```
Since `TKey` will have to be `IEquatable<TKey>`, we have special wrapper `EnumKey<TEnum>` for enums.
```csharp
public struct EnumKeyComponent : IKeyComponent<EnumKey<YourEnum>>
{
    public EnumKey<YourEnum> key { get; set; }
}
```

### Defining Primary Key
First you need to add `IPrimaryKeyRow<TComponent>` so we can tell which row has primary keys.
```csharp
public class CharacterRow : DescriptorRow<CharacterRow>, IPrimaryKeyRow<PlayerComponent> { }
```

Then you can define `PrimaryKey<TComponent>` in your Schema.
```csharp
public class GameSchema : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new();

    public readonly PrimaryKey<PlayerComponent> Player = new();

    public GameSchema()
    {
        Character.AddPrimaryKeys(Player);
        Player.SetPossibleKeys(Enumerable.Range(0, 10).ToArray());
    }
}
```
You have to provide possible keys because Groups must be built static. If you need unlimited number of keys then you'll need to go for `Index` instead.

### Defining Multiple Primary Keys
You can add as much `Primary Key` as you want for a `Table`. But you don't wanna end up one entity per `Group`, because if memory group is too fragmented you'll lose performance benetfit of ECS. You can consider some of `Primary Keys` to `Index` for secondary indexing.
```csharp
public enum CharacterState { Alive, Dead }

public class GameSchema : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new();
    public readonly Table<ItemRow> Item = new();

    public readonly PrimaryKey<PlayerComponent> Player = new();
    public readonly PrimaryKey<StateComponent> State = new();

    public GameSchema()
    {
        Character.AddPrimaryKeys(Player, State);
        Item.AddPrimaryKeys(Player);

        Player.SetPossibleKeys(Enumerable.Range(0, 10).ToArray());
        State.SetPossibleKeys(CharacterState.Alive, CharacterState.Dead);
    }
}
```

### Query with Primary Key
You can use `Where` query to find entities with specific Primary Key.
```csharp
foreach (var result in indexedDB.Select<CharacterSet>()
                            .From(schema.Character)
                            .Where(schema.Player.Is(1)))
```

### Updating Primary Key
You can initialize the `IKeyComponent` with `EntityInitializer.Build` and your entity will automatically assigned to correct `Group`.

But when you update Primary Key, it requries to call `IndexedDB.Update`. This will assign key and mark your entity pending to change groups.
```csharp
indexedDB.Update(ref result.set.player[i], result.egid[i], playerID);
```
To actually apply the changes, you need to call `IndexedDB.Step()`. **This should be called before any entity submission**. You can treat `IndexedDB` as a `IStepEngine` and add to `SortedEnginesGroup` or `UnsortedEnginesGroup`.
```csharp
indexedDB.Step();
submissionScheduler.SubmitEntities();
```

### Summary
We saw how to define `Primary Key` to split `Table`. In [Next Document](basic-indexes.md), we'll see how to define `Index`, and how to select Entities over it.
