## Naming Convention Recommendations
Below is naming convention suggestions to make schema more readable.

### For Rows
* Use `INounRow` or `IAdjectiveRow` for `Interface Rows`.
* Use `NounRow` for `Descriptor Rows`.

### For Tables
* Use `SingularNoun` for `Table`.
* Use `PluralNouns` for `Tables`.

### For Schemas
* Use `Adjective` or `SingularNoun` for Schema object. e.g. `Flying`, so you can access like `Flying.Monster`
* Use `Adjective` or `PluralNouns` for `Ranged<TSchema>`

### For Indexes
* Use `IIndexedNoun` for `Indexed Rows`. e.g. `IIndexedItemHodler`
* Use `Noun` for `Index`. e.g. `ItemHolder`

### Etc.
* Use `Indexes` as plural form for `Index` in schema.
* Use `indices` as plural form for index of array.
