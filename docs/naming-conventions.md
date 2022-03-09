## Naming Convention Recommendations
Below is naming convention suggestions to make schema more readable.

### For Result Sets
* Use `*Noun* + Set` for `Result Sets`. e.g. `DamgableSet`

### For Rows
* Use `I + *Noun* + Row` or `I + *Adjective* + Row` for `Interface Rows`. e.g. `IDamagableRow`
* Use `*Noun* + Row` for `Descriptor Rows`. e.g. `CharacterRow`

### For Tables
* Use `*SingularNoun*` for `Table<T>`.
* Use `*PluralNouns*` for `Tables<T>` or `CombinedTables<T>`.

### For Schemas
* Use `*Adjective*` or `*SingularNoun*` for Schema object. e.g. `Flying`, so you can access like `Flying.Monster`
* Use `*Adjective*` or `*PluralNouns*` for `Ranged<TSchema>`

### For Indexes and State Machines
* Use `*EntityName* + *Noun*` for `Index<T>` instance in Schema. e.g. `ItemHolder`
* Use `*EntityName* + FSM` for `StateMachine<T>`. e.g. `CharacterFSM`

### Etc.
* Use `Indexes` as plural form for `Index` in schema.
* Use `indices` as plural form for index of array.
