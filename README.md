[![Nuget](https://img.shields.io/nuget/v/Svelto.ECS.Schema)](https://www.nuget.org/packages/Svelto.ECS.Schema/) [![GitHub](https://img.shields.io/github/license/cathei/Svelto.ECS.Schema)](https://github.com/cathei/Svelto.ECS.Schema/blob/master/LICENSE) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/Dvak3QMj3n)

## Svelto.ECS.Schema
Extension for [Svelto.ECS](https://github.com/sebas77/Svelto.ECS), helps defining structure like database schema.

### Motivation
Svelto.ECS is an awesome project, however I found understanding underlying entity structure can be pretty confusing to new users like myself.
It has powerful tools like groups and filters, but lacks of wrapper to make it intutive.
I thought it will be much easier to understand group with structured Schema, and it is worth to make your code flexible, design change proof.
That is the initial motivation I wrote this Svelto.ECS.Schema extension.

Now if you wonder why this extensions named Schema, think of a RDBMS schema, there is tables, rows, columns, indexes. ECS is basically in-memory database but faster.
In RDBMS, tables can hold records having specific combination of columns.
In Svelto.ECS, groups can hold entities having specific combination of components.
That is why I chose to take friendly terms from RDBMS and define Schema of ECS.

### Features
* RDBMS-like Schema Definition with Extendible, Nestable Layout.
* Abstracted Interface Definition to define how Engines should see Entities.
* LINQ-like Querying, with Extra Type Safety.
* [Indexing Entities](#index-usage) and Automatic Tracking over Tables.
* [Finite State Machine](#state-machine-usage) with Transitions, Conditions and Callbacks.
* Ensures Zero-allocation for Frequently Called Critical Pathes.

## Getting Started
### Install
Currently it is alpha stage. You can clone this repository, or reference with [NuGet](https://www.nuget.org/packages/Svelto.ECS.Schema/). While I don't recommend to use it on production, feel free to try it and please share me the experience!

### Need help?
If you need help or want to give feedback, you can either join [my Discord Channel](https://discord.gg/Dvak3QMj3n) or ping @cathei from [Svelto's Official Discord Channel](https://discord.gg/3qAdjDb).

## Documentations
### Concept
![What is ECS?](docs/concept-ecs.md)
![What is Svelto.ECS?](docs/concept-svelto.md)
![What is Svelto.ECS.Schema?](docs/concept-schema.md)

### Basing Usages
![Defining Rows](docs/basic-rows.md)
![Defining Schemas](docs/basic-schemas.md)
![Using Queries](docs/basic-queries.md)
![Using Indexes](docs/basic-indexes.md)
![Using State Machines](docs/basic-state-machines.md)

### Advanced Usages
![Advanced Row Usages](docs/advanced-rows.md)
![Advanced Schema Usages](docs/advanced-schemas.md)
![Advanced Index Usages](docs/advanced-schemas.md)

### Etc.
![Naming Convention Recommendations](docs/naming-conventions.md)
