## Naming Convention Recommendations
Below is naming convention suggestions to make schema more readable.

### For Result Sets
* Use `*Noun* + Set` for `Result Sets`. e.g. `DamgableSet`

### For Rows
* Use `I + *Noun* + Row` or `I + *Adjective* + Row` for `Interface Rows`. e.g. `IDamagableRow`
* Use `*Noun* + Row` for `Descriptor Rows`. e.g. `CharacterRow`

### For Tables
* Use `*SingularNoun*` for `Table<T>`.
* Use `*PluralNouns*` for `CombinedTables<T>`.

### For Primary Keys, Foreign Keys and Indexes
* Use `*EntityName* + *Noun*` for `PrimaryKey<T>` or `Index<T>`. e.g. `ItemHolder`
* Use `*EntityName* + *Noun*` for `ForeignKey<T>`. e.g. `ChaserTarget`

### Etc.
* Use `*EntityName* + FSM` for `StateMachine<T>`. e.g. `CharacterFSM`
* Use `Indexes` as plural form for `Index` in schema.
* Use `indices` as plural form for index of array.
