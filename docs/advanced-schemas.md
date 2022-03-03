## Advanced Schema Usages
### Extending Schema
In advance, you can extend your Schema with inheritance, or having multiple Schemas within same `EnginesRoot`. You can still share `IndexedDB` between schemas. Good thing is, underlying groups will remain static and unique per added Schema type.

```csharp
public abstract class GameModeSchemaBase : IEntitySchema
{
    public readonly Ranged<PlayerSchema> Players;

    public GameModeSchemaBase(int playerCount)
    {
        Players = new Ranged<PlayerSchema>(playerCount);
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
_schema.CharacterByController.Query(0).Union(_indexedDB, _schema.Memo);
_schema.CharacterByController.Query(3).Union(_indexedDB, _schema.Memo);
_schema.CharacterByController.Query(6).Union(_indexedDB, _schema.Memo);

_schema.CharacterByState.Query(CharacterState.Happy).Intersect(_indexedDB, _schema.Memo);
```
Note that you have to clear `Memo<T>` before you reuse it! `Memo<T>` does not have any guarantee to have valid indices after entity submission.
