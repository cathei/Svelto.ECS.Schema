## Using Queries
Your Engines will query through various Entities. Now you can pass Schema object result from `AddSchema` to your preferred engine. Let's see how to iterate over Entities we built.

### Basic Query from single Table
In Schema extensions. Queries are formed like SQL. If you want to components of `IDamagableRow`, which is Selector Row for `(HealthComponent, DefenseComponent)`, think about form of SQL.
```csharp
var (health, defense, count) = _indexedDB.Select<IDamagableRow>().From(_schema.Character).Entities();
```
Which resembles simple SQL like this.
```sql
SELECT IDamagbleRow FROM CharacterTable;
```
Note that `CharacterRow`, Descriptor Row of `_schema.Character`, have to implement `IDamagableRow`. But don't worry, if it's not then query is not valid, **it will result in compile-time error.** Which gives type-safety layer to Schema extensions.

### Basic Query from multiple Table
If you want to iterate over multiple Tables, just pass `Tables` to the `From()` query instead.
```csharp
foreach (var ((health, defense, count), table) in _indexedDB.Select<IDamagableRow>().From(_schema.Characters).Entities())
```
Note that you'll iterate it over foreach, and `Table` variable is included as iteration variable.

### Basic Query from all Table
Simply, if you want to query from all Tables, call `All()` instead of `From()`.
```csharp
foreach (var ((health, defense, count), table) in _indexedDB.Select<IDamagableRow>().All().Entities())
```
In this case, you'll iterate any `Table` in your schema that their Row implementing `IDamagableRow`. Important note: If the their Row does not implements `IDamagableRow`, then that Table does not included in result of this query. **Even though Row has both `HealthComponent` and `DefenseComponent`.** This is important because it is different behavour from Svelto or other ECS frameworks. **In Schema extensions Rows define behaviours, not the Components.**

### Basic Query to find Tables
But what if you want to find specific type of Tables that is not Selector Row? You can query like this.
```csharp
var tables = _indexedDB.Select<CharacterRow>().Tables();

foreach (var ((health, defense, count), table) in _indexedDB.Select<IDamagableRow>().From(tables).Entities())
```
You query Tables of `CharacterRow`, and pass it to `From()`. You can find Tables with any Rows including Interface Rows and Descriptor Rows.

### Summary
We saw how to select Entities with Queries. In [Next Document](basic-indexes.md), we'll see how to define Index, and how to select Entities having specific value in a Component.
