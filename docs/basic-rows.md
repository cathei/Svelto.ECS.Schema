## Defining Rows
Rows are most basic unit in Schema extensions. Rows must define how Engines see Entities, which traits that Entities have in common, and finally what Entities are in your game design domain.

### Result Sets
`Result Set`s are composition of components that a Engine will process.  You can consider these as unit of components you can query. In Svelto.ECS you query entities like this.
```csharp
var (component1, component2, component3, count) = entitiesDB.QueryEntities<Component1, Component2, Component3>(characterGroup);
```

In Schema Extensions, you must define Seletor Row and then you can query.
```csharp
// the components you're using in your engine
public struct Result123Set : IResultSet<Component1, Component2, Component3>
{
    public NB<Component1> component1;
    public NB<Component2> component2;
    public NB<Component3> component3;

    // required by IResultSet
    public int count { get; set; }

    public void Init(in EntityCollection<Component1, Component2, Component3> collection)
        => (component1, component2, component3, count) = collection;
}

// you can access result set through `result.set`
// it is an compile-time error if characterTable does not contain ISelect123Row
var result = indexedDB.Select<ISelect123Row>().From(characterTable).Entities();
```
Fundametally, it helps writing 'good code' because the set of components you query always should have meanings. You'll always have to define how Engines should see Entities, and what they know about Entities.

### Descriptor Rows
`Descriptor Row`s represent `Entity Descriptor`, they are root of Rows. Descriptor Rows should be sealed, and not meant to be extended. They are the Rows you will eventually insert into Tables. In Svelto, simple Entity Descriptors are define as below.
```csharp
public class CharacterDescriptor : GenericEntityDescriptor<PositionComponent, SpeedComponent, HealthComponent> { }
```
In Schema extensions, Descriptor Rows are defined like this. Note than only one PositionComponent will be included in CharacterRow since they are same type.
```csharp
// result sets
public struct MoveableSet : IResultSet<PositionComponent, SpeedComponent> { ... }
public struct DamagableSet : IResultSet<PositionComponent, HealthComponent> { ... }

// descriptor row can contain multiple result sets by implementing IQueryableRow
// a entity from this descriptor row will have PositionComponent, SpeedComponent, HealthComponent
public sealed class CharacterRow : DescriptorRow<CharacterRow>,
    IQueryableRow<MovableSet>,
    IQueryableRow<DamagableSet>
{ }
```
As you can see, DescriptorRow doesn't even need to know what Component it has. It will focus on what Result Sets it should include. In other words, it will make you focus more on logic than a data.

### Interface Rows
`Interface Row`s are something you can define as a grouping of different Descriptor Rows. You cannot make descriptor with them, but you can use it represent common trait of Entities.
```csharp
// common traits Descriptor Rows implementing
public interface ICharacterRow : IQueryableRow<MovableSet>, IQueryableRow<DamgableSet> {}
// all enemies are characters
public interface IEnemyRow : ICharacterRow, IQueryableRow<EnemySpawnSet> {}

// 'hero' is character with special ability
public sealed class HeroRow : DescriptorRow<HeroRow>, ICharacterRow, ISpecialAbilityRow {}
// ground enemy doens't have trait besides common enemy traits
public sealed class GroundEnemyRow : DescriptorRow<GroundEnemyRow>, IEnemyRow {}
// flying enemy is enemy with flying trait
public sealed class FlyingEnemyRow : DescriptorRow<FlyingEnemyRow>, IEnemyRow, IFlyingRow {}
```
By making layers with Interface Rows, you'll have flexiblity to define and modify traits used over multiple Descriptor Rows.

### Summary
We looked into how to define Result Sets and Rows. [Next Document](basic-schemas.md) we'll see how to define Schemas using Rows we defined.

