## Using Queries
Your Engines will query through various Entities. Now you can pass Schema object result from `AddSchema` to your preferred engine. Let's see how to iterate over Entities we built.

### Basic Query from Table
In Schema extensions. Queries are formed like SQL. If you want to components of `DamagableSet`, which is Result Set for `<HealthComponent, DefenseComponent>`, think about form of SQL.
```csharp
foreach (var result in _indexedDB.Select<DamagableSet>().From(_schema.Character))
```
Which resembles simple SQL like this.
```sql
SELECT IDamagbleRow FROM CharacterTable;
```
Note that `CharacterRow`, Descriptor Row of `_schema.Character`, have to implement `DamagableSet`. But don't worry, if it's not then query is not valid, **it will result in compile-time error.** Which gives type-safety layer to Schema extensions.

Also it is worth to mention that you'll have to do `foreach` loop for any queries even though there is only one `Group` in the `Table`. This way you can keep your code not dependent how the underlying `Group` is managed.

### Basic Query from all Tables
Simply, if you want to query from all Tables, call `FromAll()` instead of `From()`.
```csharp
foreach (var result in _indexedDB.Select<DamagableSet>().FromAll())
```
In this case, you'll iterate any `Table` in your schema that their Row implementing `IQueryable<DamagableSet>`. Important note: If the their Row does not implements `IQueryable<DamagableSet>`, then that Table does not included in result of this query. **Even though Row has both `HealthComponent` and `DefenseComponent`.** This is important because it is different behavour from Svelto or other ECS frameworks. **In Schema extensions Rows define behaviours, not the Components.**

### Basic Query to find Tables
But what if you want to find specific type of Tables only, not all `DamagableSet`? You can query like this.
```csharp
var tables = _indexedDB.FindTables<CharacterRow>();

foreach (var result in _indexedDB.Select<DamagableSet>().From(tables))
```
You query Tables of `CharacterRow`, and pass it to `From()`. You can find Tables with any Rows including Interface Rows and Descriptor Rows.

There is also shortcut for above code, using `FromAll<T>()`.
```csharp
foreach (var result in _indexedDB.Select<DamagableSet>().FromAll<CharacterRow>())
```

### Summary
We saw how to select Entities with Queries. In [Next Document](basic-pks.md), we'll see how to define `Primary Key`, that splits a `Table` into multiple `Group`.
