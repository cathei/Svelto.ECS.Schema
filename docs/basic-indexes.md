## Using Indexes
Index is wrapper of Filter in Svelto, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a child or entire schema. Let's take a look.

### Defining Index
To define a Index, first define a `IIndexedRow<,>`.

```csharp
public interface IIndexedController : IIndexedRow<int, IIndexedController.Tag>
{
    public struct Tag : ITag {}
}
```
IIndexedRow represent a indexable trait of a Entity. First type parameter is equatable value type that will be used as key of Index. Second type parameter is to ensure uniqueness of the generic class members (We have to define `struct` to pass as type parameter, we cannot pass `class` or `interface` due to Svelto and Unity Burst Compiler limitation).

`IIndexedRow` has nested types of `Component` and `Index`. Now, let's add `IIndexedController` to your Descriptor Row.
```csharp
public sealed class CharacterRow : DescriptorRow<CharacterRow>, IDamagableRow, IIndexedController { }
```
`IIndexedRow.Component` is a special component holds the `Value` to index, and ensures that indexes are up-to-date. It has the first type parameter of `IIndexedRow`, which is `int` here, as member `Value`, but you cannot change the `Value` directly. Instead you need to call `IndexedDB.Update(ref Component, TValue)`.

Before look how to query with indexes, Let's add `IIndexedController.Index` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    public readonly Table<CharacterRow> FlyingCharacter = new Table<CharacterRow>();
    public readonly Table<CharacterRow> GroundCharacter = new Table<CharacterRow>();

    public readonly IIndexedController.Index CharacterController = new IIndexedController.Index();
}
```
`IIndexedController.Index` will index paired `IIndexedController` in any tables within declared Schema. Any child Schema will be indexed as well. Since `CharacterController` Index is defined in root Schema, any Tables with `IIndexedController` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` Tables will be indexed. If you want to index specific Tables only, define a child Schema.

Also, you can share `IIndexedController` across different descriptors. Index will handle them well.

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
Also the group you provieded have to implement both `IDamagableRow` and `IIndexedController`. Otherwise the Query will emit compile-time error.

### Querying Indexes from all Tables
Same as other Queries, you could pass Tables or CombinedTables to `From()`.
```csharp
var tables = indexedDB.Select<CharacterRow>().Tables();

foreach (var ((health, position, count), indices, table) in indexedDB.Select<IDamagableRow>().From(tables).Where(schema.CharacterController, 3).Entities())
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
