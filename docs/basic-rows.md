## Defining Rows
Rows are most basic unit in Schema extensions. Rows must define how Engines see Entities, which traits that Entities have in common, and finally what Entities are in your game design domain.

There is three level of Rows as it extends.

### Selector Rows
`Selector Row`s are minimal blocks, leaf nodes of Rows. You can consider these as unit of components you can query. Specifically, in Svelto.ECS you query entities like this.
```csharp
var (component1, component2, component3, count) = entitiesDB.QueryEntities<Component1, Component2, Component3>(characterGroup);
```
In Schema Extensions, you should first define Seletor Row and then you can query.
```csharp
public interface ISelect123Row : ISelectorRow<Component1, Component2, Component3> {}

// yes, the return value is exactly same as when you're using plain Svelto
var (component1, component2, component3, count) = indexedDB.Select<ISelect123Row>().From(characterTable).Entities();

// below is valid, but not recommended at all. Always define as own selector row that has meaning.
// var (component1, component2, component3, count) = indexedDB.Select<ISelectorRow<Component1, Component2, Component3>>().From(characterTable).Entities();
```
Fundametally, it helps writing 'good code' because the set of components you query always should have meanings. You'll always have to define how Engines should see Entities, and what they know about Entities.

### Descriptor Rows
`Descriptor Row`s represent `Entity Descriptor`, they are root of Rows. Descriptor Rows should be sealed, and not meant to be extended. They are the Rows you will eventually insert into Tables. In Svelto, simple Entity Descriptors are define as below.
```csharp
public class CharacterDescriptor : GenericEntityDescriptor<PositionComponent, SpeedComponent, HealthComponent> { }
```
In Schema extensions, Descriptor Rows are defined like this. Note than only one PositionComponent will be included in CharacterRow since they are same type.
```csharp
// selector rows
public interface IMovableRow : ISelectorRow<PositionComponent, SpeedComponent> {}
public interface IDamagableRow : ISelectorRow<PositionComponent, HealthComponent> {}

// descriptor row implements multiple selector rows
// a entity from this descriptor row will have PositionComponent, SpeedComponent, HealthComponent
public sealed class CharacterRow : DescriptorRow<CharacterRow>, IMovableRow, IDamagableRow {}
```
As you can see, DescriptorRow doesn't even need to know what Component it has. It will focus on what Selector Rows it should include. In other words, it will make you focus more on logic than a data.

### Interface Rows
`Interface Row`s are middle-mans, internal nodes of the Rows. You cannot Query with them, you cannot make descriptor with them, but what you can is using it as grouping of Seletor Rows commonly implemented.
```csharp
// common traits Descriptor Rows implementing
public interface ICharacterRow : IMovableRow, IDamagableRow {}
// all enemies are characters
public interface IEnemyRow : ICharacterRow, ISpawnableRow {}

// 'hero' is character with special ability
public sealed class HeroRow : DescriptorRow<HeroRow>, ICharacterRow, ISpecialAbilityRow {}
// ground enemy doens't have trait besides common enemy traits
public sealed class GroundEnemyRow : DescriptorRow<GroundEnemyRow>, IEnemyRow {}
// flying enemy is enemy with flying trait
public sealed class FlyingEnemyRow : DescriptorRow<FlyingEnemyRow>, IEnemyRow, IFlyingRow {}
```
By making layers with Interface Rows, you'll have flexiblity to define and modify traits used over multiple Descriptor Rows.

### Summary
We looked into how to define Rows. [Next Document](basic-schemas.md) we'll see how to define Schemas using Rows we defined.

