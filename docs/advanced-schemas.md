## Advanced Schema Usages
### Query with Entity ID
Sometimes you'll want to query with specific entity ID. You can use `EntityID.Is` with `Where` query. Note that even though you query one Entity ID, you still need to loop through it by design.

```csharp
foreach (var result in indexedDB.Select<ResultSet>()
                                .FromAll<CharacterRow>()
                                .Where(EntityID.Is(targetID)))
```
It count as `|| (or)` condition if you have multiple entity ID constraints of query. Other conditions will work as `&& (and)` condition.

### Using Interfaces
Schema extensions defines few convariant interfaces so users can easily access to abstracted Tables. For example, if `HeroRow` and `EnemyRow` both implements `ICharacterRow`, you can do this.
```csharp
public class GameSchema

Table<HeroRow> heroTable = new Table<HeroRow>();
Table<EnemyRow> enemyTable = new Table<EnemyRow>();

// you have common interface, you can treat it as same type of table!
IEntityTable<ICharacterRow> charcterTable1 = heroTable;
IEntityTable<ICharacterRow> charcterTable2 = enemyTable;
```
And it is still type safe when you do queries! In other words with `IEntityTable<ICharacterRow>` you can only query with Result Sets ICharacterRow implements.

## Reactive Engine Usage
To get notified when a Entity with specific Row is added, removed or swapped, you can define a Row extending `IReactiveRow<TComponent>`. And create a engine extending `ReactToRowEngine<TRow, TComponent>`.

### Extending Schema
In advance, you can extend your Schema with inheritance, or having multiple Schemas within same `EnginesRoot`. You can still share `IndexedDB` between schemas. Good thing is, underlying groups will remain static and unique per added Schema type.

```csharp
public abstract class GameModeSchemaBase : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new();

    public readonly PrimaryKey<PlayerComponent> Player = new();

    public GameModeSchemaBase(int playerCount)
    {
        Character.AddPrimaryKeys(Player);
        Player.SetPossibleKeys(Enumerable.Range(0, playerCount).ToArray());
    }
}

public class PvPGameModeSchema : GameModeSchemaBase
{
    // eight player max
    public PvPGameModeSchema() : base(8) { }
}

public class CoOpGameModeSchema : GameModeSchemaBase
{
    public readonly Table<CharacterRow> Enemy = new();

    // two player max
    public CoOpGameModeSchema() : base(2) { }
}
```

### Multiple Root Schemas
You can add more than one Root Schemas in a `IndexedDB`. You can consider spliting Schemas, if Entities in two Schemas does not interact at all.

### Calculate Union and Intersection of Indexes
To calculate Union and Intersection of Indexes, you can use temporary filters called `Memo<TRow>`. It can be included anywhere in Schema. Define it like this:
```csharp
public class MemoSchema : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new Table<CharacterRow>();

    public readonly Index<CharacterController> CharacterController = new Index<CharacterController>();

    public readonly Index<CharacterEmotion> CharacterEmotion = new Index<CharacterEmition>();

    public readonly Memo<ICharacterRow> Memo = new Memo<ICharacterRow>();
}
```
Use it like this:
```csharp
_indexedDB.Memo(_schema.Memo).Clear();

_indexedDB.Memo(_schema.Memo).Union(_schema.CharaterController, 0);
_indexedDB.Memo(_schema.Memo).Union(_schema.CharaterController, 3);
_indexedDB.Memo(_schema.Memo).Union(_schema.CharaterController, 6);

_indexedDB.Memo(_schema.Memo).Intersect(_schema.CharacterEmotion, Emotion.Happy);

var ((controller, count), indices) = _indexedDB
    .Select<ICharacterControllerSelector>().From(_schema.Character).Where(_schema.Memo).Entities();
```
Note that you have to clear `Memo<T>` before you reuse it! `Memo<T>` does not have any guarantee to have valid indices after entity submission.