## Using Indexes
### Defining Indexes
Index is wrapper of filters system, but works like indexes in RDBMS. Filters are used to have subset from a group. Indexes are to collect entities by specific key, from a child or entire schema. Let's take a look. We'll start from defining `IndexTag`.
```csharp
public class CharacterController : IndexTag<int, CharacterController.Unique>
{
    public struct Unique : IUnique { }
}
```
IndexTag represent a indexable trait of entity. First type parameter is equatable value type that will be used as key of Index. Second type parameter is to ensure uniqueness of the generic class members (We have to define struct to pass as type parameter, due to Svelto limitation).

`IndexTag` has nested types of `Component` and `Index`. Now, let's add `CharacterController.Component` to your descriptor.
```csharp
public class CharacterDescriptor<HealthComponent, PositionComponent, CharacterController.Component> { }
```
`IndexTag.Component` is a special component holds the `Value` to index, and ensures that indexes are up-to-date. It has the first type parameter of `IndexTag`, which is `int` here, as member `Value`, but you cannot change the `Value` directly. Instead you need to call `Update(IndexedDB, TValue)`.

Before look how to query with indexes, Let's add `CharacterController.Index` to our schema.
```csharp
public class IndexedSchema : IEntitySchema
{
    public readonly Table<CharacterDescriptor> FlyingCharacter = new Table<CharacterDescriptor>();
    public readonly Table<CharacterDescriptor> GroundCharacter = new Table<CharacterDescriptor>();

    public readonly CharacterController.Index CharactersByController = new CharacterController.Index();
}
```
`IndexTag.Index` will index paired `IndexTag.Component` component in any tables within declared schema. Any child schema will be indexed as well. Since `CharacterController.Index` is defined in root schema, any table with `CharacterController.Component` will be indexed. In this example both `FlyingCharacter` and `GroundCharacter` group will be indexed and returned when queried. If you want to index specific groups only, define a child Schema.

Also, you can share `IndexTag.Component` across different descriptors. Index will handle them well.

### Querying Indexes
Now, finally you can iterate over entities with `IndexTag.Index`. You don't have to include `IndexTag.Component` in the type list. You can query any type of component within the descriptor, because as long as you keep a group with single descriptor you can iterate with same filter.

Just like how you query Entities in `Table`, you can query with `IndexedDB`. To query entites with `IndexTag.Component.Value` of 3:
```csharp
foreach (var ((health, position, count), indices, group) in schema.CharactersByController
    .Query(3).Entities<HealthComponent, PositionComponent>(indexedDB))
{
    foreach (var i in indices)
    {
        health[i].current += 10;
    }
}
```
Note that you can use foreach loop to iterate indices.  but **DO NOT** update `IndexTag.Component` component while iterating through index query with it. It is undefined behaviour. If you have to, consider using `StateMachine` instead, which will be explained later.

If you want to query index within specific `Table<T>` or `Tables<T>`, use `From` like this:
```csharp
var ((health, position, count), indices) = schema.CharactersByController
    .Query(3).From(schema.FlyingCharacter).Entities<HealthComponent, PositionComponent>(indexedDB);
```
