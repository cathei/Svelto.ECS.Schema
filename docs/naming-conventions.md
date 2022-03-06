## Naming Convention Recommendations
Below is naming convention suggestions to make schema more readable.

### For Rows
* Use `INounRow` or `IAdjectiveRow` for `Interface Rows`. e.g. `IDamagableRow`
* Use `NounRow` for `Descriptor Rows`. e.g. `CharacterRow`

### For Tables
* Use `SingularNoun` for `Table<T>`.
* Use `PluralNouns` for `Tables<T>` or `CombinedTables<T>`.

### For Schemas
* Use `Adjective` or `SingularNoun` for Schema object. e.g. `Flying`, so you can access like `Flying.Monster`
* Use `Adjective` or `PluralNouns` for `Ranged<TSchema>`

### For Indexes
* Use `EntityName + Noun` for `IIndexKey<T>`. e.g. `ItemHolder`
* Use same name for `Index<T>` instance in Schema.

### For State Machines
* Use `EntityName + State` for `IStateMachineKey<T>`. e.g. `CharacterState`
* Use `EntityName + FSM` for `StateMachine<T>`. e.g. `CharacterFSM`

### Etc.
* Use `Indexes` as plural form for `Index` in schema.
* Use `indices` as plural form for index of array.
