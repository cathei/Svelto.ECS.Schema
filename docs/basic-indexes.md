## Using Indexes
Index is wrapper of Filter in Svelto, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a child or entire schema. Let's take a look.

### Defining Index
To define a Index, first define a `IKeyComponent<TKey>`. It can be used as `Primary Key` or `Index`.

```csharp
public struct PlayerComponent : IIndexableComponent<int>
{
    public int key { get; set; }

    public PlayerComponent(int playerID) => key = playerID;
}
```
The `TKey` parameter is the type you'd like to use as a key, we used `int` here.

`key` is a special property for `Index`, and ensures that indexes are up-to-date. You should only directly assign it when initialization. After submission, instead you need to call `IndexedDB.Update`.

Like the `IQuerableRow`, you need to add `IIndexableRow<TComponent>` to your Descriptor Row, to make it possible to index.

```csharp
public sealed class CharacterRow :
    DescriptorRow<CharacterRow>,
    IQueryableRow<DamagableSet>
    IIndexableRow<CharacterControllerComponent>,
{ }
```

Before look how to query with indexes, Let's also add `Index<TComponent>` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    public readonly Table<CharacterRow> FlyingCharacter = new Table<CharacterRow>();
    public readonly Table<CharacterRow> GroundCharacter = new Table<CharacterRow>();

    public readonly Index<CharacterControllerComponent> CharacterController = new Index<CharacterControllerComponent>();
}
```
`Index<IndexableComponent>` will index paired `IndexableComponent` in any tables within declared Schema. Any child Schema will be indexed as well. Since `CharacterController` Index is defined in root Schema, any Tables with `IndexableComponent` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` Tables will be indexed. If you want to index specific Tables only, define a child Schema.

It is safe to share `IndexableComponent` across different Descriptor Rows. Index will handle them well.

### Querying Indexes from Single Table
Now, finally you can iterate over entities with `CharacterControllerComponent`. You can do this by adding `Where(Index.Is(Key))` to your Query, just like how you will do in SQL. For example below will query Entites where `IIndexedController.Component.Value` is 3.
```csharp
var result = indexedDB.Select<DamagableSet>().From(schema.FlyingCharacter).Where(schema.CharacterController.Is(3)).Entities();
```
Which looks like this SQL.
```sql
SELECT DamgableSet FROM FlyingCharacterTable WHERE CharacterController = 3;
```
Note that you have access to `result.indices`. You'll have to loop through this to iterate filtered Entities by Index.
```csharp
foreach (var i in result.indices)
{
    result.set.health[i].current += 10;
}
```
Also the Table you provieded have to implement both `IDamagableRow` and `IndexedRow<CharacterController>`. Otherwise the Query will emit compile-time error.

### Querying Indexes from all Tables
Same as other Queries, you could pass Tables or CombinedTables to `From()`, or use `FromAll<T>`. In this case you must specify `TRow`, common decendent of `IDamagableRow` and `IndexedRow<CharacterController>`.
```csharp
foreach (var result in indexedDB.Select<DamagableSet>().FromAll<CharacterRow>().Where(schema.CharacterController.Is(3)).Entities())
{
    foreach (var i in result.indices)
    {
        result.set.health[i].current += 10;
    }
}
```
Note that you can use foreach loop to iterate indices.

### Summary
We learned how to define and iterate through Indexes. Lastly, it is valid to call `IndexedDB.Update` to filter while iterating it, **only if you're changing the currently iterating index**. Otherwise it will result in undefined behaviour. Or you could consider using `StateMachine` instead, which will be the [Next Document](advanced-state-machines.md).
