## Using Foreign Keys
When you develop a game you'll find lots of case that you need a relation between entities. A monster following target, equipments associated with character, collisions between objects, something like that. In RDBMS the relationship is made with `Foreign Key` and `Join` query. Schema extensions have `Foreign Key` feature to mimic this behaviour.

### Defining Foreign Key
First define a `IForeignKeyComponent` to save reference to other entity.
```csharp
public struct EquipComponent : IForeignKeyComponent
{
    public EntityReference reference { get; set; }
}
```
Then add `IForeignKeyRow<T>` to specify the `Row` you will reference from.  Also add `IReferenceabeRow<T>` to specify the `Row` you will reference to. Both side of settings are required to ensure type safety.

```csharp
public interface IEquipmentRow : IForeignKeyRow<EquipComponent> { }
public interface ICharacterRow : IReferencableRow<EquipComponent> { }
```

Finally, add `ForeignKey<TComponent, TTargetRow>` to your schema.
```csharp
public class GameSchema : EntitySchema
{
    public readonly Table<EquipmentRow> Equipment = new();
    public readonly Table<CharacterRow> Character = new();

    public readonly ForeignKey<EquipComponent, CharacterRow> Equipped = new();
}
```

### Reference other Row
You can set reference while entity initialization, or call `IndexedDB.Update` to set reference. You can pass target EGID or EntityReference. If you wanna remove the reference, pass `EntityReference.Invalid`.
```csharp
indexedDB.Update(ref result.set.equip[i], result.egid[i], otherEGID);
```

### Join Query on One-to-Many relationship
Now you can query the equipments equipped to a character. It is typical one-to-many relationship. Since multiple equipments can be equipped to a character, while iterating indices you will encounter up to one equipment, but you can encounter same character more than one time.

```csharp
foreach (var result in indexedDB.Select<EquipmentSet>()
                                .From(schema.Equipment)
                                .Join<CharacterSet>().On(schema.Equipped))
{
    foreach (var (ia, ib) in result.indices)
    {
        ref var equipment = ref result.setA.equipment[ia];
        ref var character = ref result.setB.character[ib];
        // ...
    }
}
```

### Join Query on Many-to-Many relationship
Now, traditionally a `Foreign Key` cannot solve many-to-many relationship alone. But cases like collision or team settings, you'll need many-to-many relationship. The solution of that is making a `Join Table`, which is a Entity that will hold references to both Entities.

To define `Join Table` you'll need two `IForeignKeyComponent`.
```csharp
public struct AttackerComponent : IForeignKeyComponent
{
    public EntityReference reference { get; set; }
}

public struct DefenderComponent : IForeignKeyComponent
{
    public EntityReference reference { get; set; }
}
```

Then set up `IForeignKeyRow` and `IReferenceableRow` accordingly. In this example `HitInfoRow` will hold attacker and defender character reference. In other cases, `IReferenceableRow` can be inherited from different Rows.
```csharp
public class ICharacterRow :
    IReferenceableRow<AttackerComponent>,
    IReferenceableRow<DefenderComponent>
{ }

public class HitInfoRow :
    DescriptorRow<HitInfoRow>,
    IQueryableRow<HitInfoSet>,
    IForeignKeyRow<AttackerComponent>,
    IForeignKeyRow<DefenderComponent>
{ }
```

Then add `Table` and `ForeignKey` to your `Schema`.
```csharp
public class GameSchema : IEntitySchema
{
    public readonly Table<CharacterRow> Character = new();
    public readonly Table<HitInfoRow> HitInfo = new();

    public readonly ForeignKey<AttackerComponent, ICharacterRow> HitAttacker = new();
    public readonly ForeignKey<DefenderComponent, ICharacterRow> HitDefender = new();
}
```

Now you can query with two Joins. You'll iterate all the Attacker-Defender pairs.
```csharp
foreach (var result in indexedDB.Select<HitInfoSet>().From(schema.HitInfo)
                                .Join<AttackerSet>().On(schema.HitAttacker)
                                .Join<DefenderSet>().On(schema.HitDefender))
{
    foreach (var (ia, ib, ic) in result.indices)
    {
        // in here, same as order of Join
        // setA is HitInfoSet of DamageInfoRow
        // setB is AttackerSet of ICharacterRow (Attacker)
        // setC is DefenderSet of ICharacterRow (Defender)
        // each set is indexed with ia, ib, ic
        result.setC.health[ic].value -= result.setB.damage[ib].value;
    }
}
```

### Query with Reverse References
For some cases, you might want to get a list of entites that referencing specific entity. Use `IndexedDB.QueryReverseReferences`.
