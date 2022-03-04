## What is Svelto.ECS?
If you're familiar with Svelto.ECS, you can skip this document though recommended.

### Concept
Svelto.ECS is fast, real data-oriented ECS framework, also one of the rigid ones. You can use it on any C# game engines, even with Unity DOTS ECS. Since it is always better to read official articles of Svelto.ECS, I won't get too deep but only explain core concepts here.

### Entity Component
`Entity Component` is to define Component in ECS. In Svelto.ECS it must be `unmanaged` struct, and you cannot add or remove a Component from Entity in runtime.
```csharp
public struct PositionComponent : IEntityComponent
{
    public int x, y, z;
}
```
You will define a component like this. Since it is `unmanged`, it cannot have any reference types including arrays. But do not afraid, Schema extensions offers many tools to help you.

### Entity Descriptor
`Entity Descriptor` is composition of `Component`, representing specific in-game object in game design domain. For example if `CharacterDescriptor` has `PositionComponent`, `SpeedComponent`, `HealthComponent`, it will be defined like this.
```csharp
public class CharacterDescriptor :
    GenericEntityDescriptor<PositionComponent, SpeedComponent, HealthComponent> { }
```
By the way, in Schema extensions it is completely replaced by `Descriptor Row`s, but it is good to know the underlying concept!

### Engines
In Svelto, Systems are called `Engine`s. Engines can be generalized or specialized. Engines should not communicate with each other. Only poll and process based on data. For details please check Svelto's official documents.

### Groups
`Group`s are set of entities. A `Group` should belong to a `Descriptor`, to iterate components over same indices. You can query entities from a Group like this.
```csharp
// inside of a Engine, query entities
var (positions, speeds, count) =
    entitiesDB.QueryEntities<PositionComponent, SpeedComponent>(Groups.MovingCharacter);

// processing entities
for (int i = 0; i < count; ++i)
    positions[i].x += speeds[i].x;
```
As you can see, iterating groups are cache-friendly and fast, therefore it should be used as primary ways to split entities. In Schema extensions Groups are wrapped by concept of `Table`, and you will strongly pair a `Descriptor Row` to a `Table`.

There is also concept of `GroupCompund` to generate combination of groups, which is completely replaced with extendible and flexible `Schema`s in Schema extensions.

### Filters
Filters are another tool to define set of entities. It stores subset of indices in a Group. But it won't be easy to use and manage it in current Svelto without Schema extensions. In Schema extensions they are wrapped by concept of `Index`.

### Summarize
We looked into very basic Svelto features and how it will change in Schema extensions. [Next document](concept-schema.md) will explain further about Schema extensions speicifc concepts.
