[![Nuget](https://img.shields.io/nuget/v/Svelto.ECS.Schema)](https://www.nuget.org/packages/Svelto.ECS.Schema/) [![GitHub](https://img.shields.io/github/license/cathei/Svelto.ECS.Schema)](https://github.com/cathei/Svelto.ECS.Schema/blob/master/LICENSE) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/Dvak3QMj3n)

# Svelto.ECS.Schema
Schema and State Machine extensions for [Svelto.ECS](https://github.com/sebas77/Svelto.ECS).

## Features
* RDBMS-like **Schema Definition** with Extendible, Nestable Layout.
* **Abstracted Interface Definition** to define how Engines should see Entities.
* **SQL-like Queries** with **Extra Type Safety**.
* **Indexing Entities** and Automatic Tracking over Tables.
* **Join** between Entities, for One-to-One, One-to-Many or Many-to-Many relationships.
* **Finite State Machine** with Transitions, Conditions and Callbacks.
* Ensures [**Zero-allocation**](https://www.sebaslab.com/zero-allocation-code-in-unity/) for Frequently Called Critical Pathes.

## Getting Started
Currently it is alpha stage. You can clone this repository, or reference with [NuGet](https://www.nuget.org/packages/Svelto.ECS.Schema/). While I don't recommend to use it on production, feel free to try it and please share me the experience!

Svelto.ECS.Schema's minimum C# version is 8. (It requires default interfaces implementation)

## Need help?
If you need help or want to give feedback, you can either join [my Discord Channel](https://discord.gg/Dvak3QMj3n) or ping @cathei from [Svelto's Official Discord Channel](https://discord.gg/3qAdjDb).

## Documentations
### Concept
* [What is ECS?](docs/concept-ecs.md)
* [What is Svelto.ECS?](docs/concept-svelto.md)
* [What is Svelto.ECS.Schema?](docs/concept-schema.md)

### Basic Usages
* [Defining Rows](docs/basic-rows.md)
* [Defining Schemas](docs/basic-schemas.md)
* [Using Queries](docs/basic-queries.md)
* [Using Primary Keys](docs/basic-pks.md)
* [Using Indexes](docs/basic-indexes.md)

### Advanced Usages
* [Using State Machines](docs/advanced-state-machines.md)
* [Using Foreign Keys and Joins](docs/advanced-fks.md)
* [Advanced Schema Usages](docs/advanced-schemas.md)

### Etc.
* [vs. Doofus Example](docs/vs-doofus.md)
* [Naming Convention Recommendations](docs/naming-conventions.md)
