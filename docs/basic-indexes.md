## Using Indexes
Index is wrapper of Filter in Svelto, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a child or entire schema. Let's take a look.

### Defining Index
To define a Index, first define a `IIndexKey<TSelf>`.

```csharp
public readonly struct CharacterController : IIndexKey<CharacterController>
{
    public readonly int _controllerID;

    public CharacterController(int controllerID) => _controllerID = controllerID;

    public static implicit operator CharacterController(int controllerID) => new CharacterController(controllerID);

    public bool KeyEquals(in CharacterController other) => _controllerID == other._controllerID;

    public int KeyHashCode() => _controllerID.GetHashCode();
}
```
IIndexKey represent a indexable key of a Entity. With it, you can access `Indexed<TKey>` and `IIndexedRow<TKey>`. It is recommended to define as readonly struct. For convenience, we defined implicit operator from inner `int` to `CharacterController`.

Now, let's add `IIndexedRow<TKey>` to your Descriptor Row, so Index can propery recognize your Table.
```csharp
public sealed class CharacterRow :
    DescriptorRow<CharacterRow>,
    IIndexedRow<CharacterController>,
    IDamagableRow
{ }
```
`Indexed<TKey>` is a special component holds the `TKey Key` property for index, and ensures that indexes are up-to-date. It has the first type parameter of `IIndexedRow`, which is `int` here, as member `Value`, but you cannot change the `Value` directly. Instead you need to call `IndexedDB.Update(ref Indexed<TKey>, TKey)`.

Before look how to query with indexes, Let's add `Index<TKey>` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    public readonly Table<CharacterRow> FlyingCharacter = new Table<CharacterRow>();
    public readonly Table<CharacterRow> GroundCharacter = new Table<CharacterRow>();

    public readonly Index<CharacterController> CharacterController = new Index<CharacterController>();
}
```
`Index<TKey>` will index paired `IndexedRow<TKey>` in any tables within declared Schema. Any child Schema will be indexed as well. Since `CharacterController` Index is defined in root Schema, any Tables with `IndexedRow<TKey>` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` Tables will be indexed. If you want to index specific Tables only, define a child Schema.

It is safe to share `IndexedRow<TKey>` across different Descriptor Rows. Index will handle them well.

### Querying Indexes from Single Table
Now, finally you can iterate over entities with `IIndexedController`. You can do this by adding `Where(Index, Value)` to your Query, just like how you will do in SQL. For example below will query Entites where `IIndexedController.Component.Value` is 3.
```csharp
var ((health, defense, count), indices) = indexedDB.Select<IDamagableRow>().From(schema.FlyingCharacter).Where(schema.CharacterController, 3).Entities();
```
Which looks like this SQL.
```sql
SELECT IDamagableRow FROM FlyingCharacterTable WHERE CharacterController = 3;
```
Note that you got additional `indices` value. You'll have to loop through this to iterate filtered Entities by Index.
```csharp
foreach (var i in indices)
{
    health[i].current += 10;
}
```
Also the Table you provieded have to implement both `IDamagableRow` and `IndexedRow<CharacterController>`. Otherwise the Query will emit compile-time error.

### Querying Indexes from all Tables
Same as other Queries, you could pass Tables or CombinedTables to `From()`, or use `FromAll<T>`. In this case you must specify `TRow`, common decendent of `IDamagableRow` and `IndexedRow<CharacterController>`.
```csharp
foreach (var ((health, position, count), indices, table) in indexedDB.Select<IDamagableRow>().FromAll<CharacterRow>().Where(schema.CharacterController, 3).Entities())
{
    foreach (var i in indices)
    {
        health[i].current += 10;
    }
}
```
Note that you can use foreach loop to iterate indices.

### Summary
We learned how to define and iterate through Indexes. Lastly, **DO NOT** update `IIndexedRow.Component` component while iterating through index query with it. It is undefined behaviour. If you have to, consider using `StateMachine` instead, which will be the [Next Document](basic-state-machines.md).
