## What is ECS?
If you're familiar with concept of ECS, you can skip this document though recommended.

### ECS
ECS is short term of [Entity-Component-System](https://en.wikipedia.org/wiki/Entity_component_system), which is data-oriented programming paradigm. ECS commonly compared with OOP. It has three basic concept, as the name says, Entity, Component and System.

### Entity
Entity has little confusing definition. Someone says "Entity is no more than just ID", and someone says "Entity is combination of Component". It depends how you see it. You could think about `object` in C#. What is `object`? It is base class for all classes. Depends how you see it, it can be from "nothing more than a instance" to everything.

If you're friendly with Unity, you can imagine very bare `GameObject`. Even without `Transform` component. You can add `Component`s to `GameObject` so it can do certain things, but `GameObject` itself does not have meaning. It is just base class for everything in game. Even name proves it ('game' + 'object').

Overall, you'll want to focus on what makes Entity special, than itself.

### Component
Component in ECS is like Unity's Component, but also very different. Component holds modular piece of data. You'll want small generalized pieces like `PositionComponent` and `VelocityComponent`, rather than specialized `CharacterMovementComponent`. Unlike OOP scenarios, Components does not holds logic. Again, it is just piece of data than you can reused over Entities and Systems.

### System
If Entities are just objects, and Components only holds data, where will logic be? Logics belong in Systems. A System process a set of Component. For example, `CharacterMovementSystem` will process set of `(PositionComponent, VelocityComponent)`. It is important than it does not bound to specific Entity. If a Entity has those set of Components, System processes. If a Entity doesn't have those set of Components, System doesn't process.

Also Systems are suppose to be very independent. They should not know other concrete System. So we can write modular, [SOLID](https://en.wikipedia.org/wiki/SOLID) code.

### Summarize
We looked about common basic concepts of ECS. Which is Enity, Component, Systems. Those are common definition, but there is also different implementation details between ECS frameworks. For example, some lets you add or remove Components on runtime, some doesn't. [Next document](concept-svelto.md) we'll look into Svelto speicfic details.
