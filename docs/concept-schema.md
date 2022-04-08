## What is Svelto.ECS.Schema?

### Motivation
Svelto.ECS is an awesome project, however I found understanding underlying entity structure can be pretty confusing to new users like myself.
It has powerful tools like groups and filters, but lacks of wrapper to make it intutive.
I thought it will be much easier to understand group with structured Schema, and it is worth to make your code flexible, design change proof.
That is the initial motivation I wrote this Svelto.ECS.Schema extension.

### Concept
Now if you wonder why this extensions named Schema, think of a RDBMS schema, there is tables, rows, columns, indexes. ECS is basically in-memory database but faster.
In RDBMS, tables can hold records having specific combination of columns.
In Svelto.ECS, groups can hold entities having specific combination of components.
That is why I chose to take friendly terms from RDBMS and define Schema of ECS.

### Rows
In Schema extensions, `Row`s are abstracted layer of traits that a Entity can have. It can be extended, layered, mxied and matched very flexiblely. Engines should query entities as `Result Set`s, while actual Entity Descriptor is defined with `Descriptor Row`s.

### Tables
`Table` is a wrapper for list of `Group` in Svelto ECS, mimicking RDBMS Tables. It is also a good replacement for `GroupCompound` if used with `Primary Key`. A `Table` is tied up to specific `Descriptor Row`. You can either define single Table, or ranged, or combined Table from defined onces. Using `IEntityTable<out TRow>` interface is a way to treat Tables with same common interface same way.

### Primary Keys
`Primary Key` splits `Table` into multiple internal `Group`, so your iteration can be optimized. `Primary Key` can used with `Index`, in flavor of performance. Generally, if you have few and commonly used keys you should prefer to use `Primary Key`.

### Indexes
`Index` is a wrapper for `Filter` in Svelto ECS, mimicking RDMBS Indexes. It saves list of entities per key, and you can query entities having speicific key.

### Foreign Keys
`Foreign Key` is a wrapper for `EntityReference`, and you can use it to make a `1-N` or `N-N` relationship. You can query them with `Join` as well!

### Schemas
`Schema` is extendible definition set of above. A Schema can contain Tables, PKs, FKs, Indexes and State Machines. While Groups must be rigid and static in Svelto, Schema is effective way to hide the limitation and make it easier to modify when game design changed.

### State Machines
Schema extensions also functionality to define [Finite State Machine](https://en.wikipedia.org/wiki/Finite-state_machine) in Svelto. You can define States, Conditions, Callbacks of FSM, and query Entities with speicific State.

### Summary
We looked into features of Schema extensions. [Next document](basic-rows.md) will teach you how to use Schema extensions, starting from defining Rows.
