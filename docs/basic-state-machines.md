## State Machine Usage
### Defining State Machine
Schema extensions support Finite State Machine (FSM) feature, which automatically changes Component state by condition for you. To define a FSM, first define a enum indicates states.
```csharp
public enum CharacterState { Normal, Angry, Fever, MAX }
```
And IStateMachineComponent class to store and index the state of the entity.
```csharp
public struct CharacterStateComponent : IStateMachineComponent<EnumKey<CharacterState>>
{
    public EGID ID { get; set; }
    public EnumKey<CharacterState> key { get; set; }

    public CharacterStateComponent(CharacterState state) : this() { this.key = state; }
}
```
The structure is same as `IIndexableComponent<TKey>`. Since the key is `enum`, which does not implement `IEquatable<T>`, we use special wrapper `EnumKey<T>`. You can use it as it is the inner enum.

Now you can define FSM class, inherit `StateMachine<TComponent>`.
```csharp
public class CharacterFSM : StateMachine<CharacterStateComponent>
{
    public interface IRow : IIndexedRow {}

    protected override void OnConfigure()
    {
        var config = Configure<IRow>();

        var stateNormal = config.AddState(CharacterState.Normal);
        var stateAngry = config.AddState(CharacterState.Angry);
        var stateFever = config.AddState(CharacterState.Fever);
    }
}
```
Same manner as `Index`, `StateMachine` accepts `StateMachineKey`.

Also define Interface Row `IRow` to represent the Rows using State Machine. `OnConfigure` method is used to configure your State Machine. Call `Configure<IRow>` to get a builder for State Machine. By calling `AddState` you can add State.

Now you have states of State Machine, but it won't have any effect until you add Transitions between States.

### Adding Transitions
Transition describes how State changes. In `OnConfigure` you can add Transition and Conditions.
```csharp
public class CharacterFSM : StateMachine<CharacterFSMState>
{
    public interface IRow : IIndexedRow,
        IEntityRow<RageComponent>,
        IEntityRow<TriggerComponent>
    {}

    protected override void OnConfigure()
    {
        var config = Configure<IRow>();

        var stateNormal = config.AddState(CharacterState.Normal);
        var stateAngry = config.AddState(CharacterState.Angry);
        var stateFever = config.AddState(CharacterState.Fever);

        stateNormal.AddTransition(stateAngry)
            .AddCondition((ref RageComponent rage) => rage.value >= 30);

        stateAngry.AddTransition(stateNormal)
            .AddCondition((ref RageComponent rage) => rage.value < 20);

        stateNormal.AddTransition(stateFever)
            .AddCondition((ref RageComponent rage) => rage.value < 10)
            .AddCondition((ref TriggerComponent trigger) => trigger.value);
    }
}
```
Note that here, IRow must include inner `IIndexedRow` and `IEntityRow<T>`s to use in Transitions and Conditions. It is important to manually do this, so you can ensure any Rows using State Machine will have all those Components.

By calling `FromState.AddTransition(ToState)` you define a Transition. You also should add Condition for Transition to happen, by calling `AddCondition`. Conditions take a lambda with single `ref IEntityComponent` parameter and `bool` return value.

All Conditions must return `true` for the Transition to be executed. If you want another set of Conditions, you can add another Transition with same States. If there are multiple Transition met the Conditions, the Transition added first in `OnConfigure()` has higher priority. 

You can also use special `config.AnyState` property to define Transition from any States.

### Adding Callbacks
If you want to set Component values when Transition happens, you can define `ExecuteOnEnter` and `ExecuteOnExit` Callbacks. Also remember to add `IEntityRow<Component>` to your `IRow`, for each Component you'll use.
```csharp
stateSpecial
    .ExecuteOnEnter((ref TriggerComponent trigger) => trigger.value = false)
    .ExecuteOnEnter((ref SpecialTimerComponent timer) => timer.value = 1)
    .ExecuteOnExit((ref RageComponent rage) => rage.value = 5);
```
Callbacks receive same parameter as Conditions, but without return value.

### Using State Machine
To use State Machine, first add `StateMachine.IRow` to your Row. That means all other components you used for Conditions and Callbacks will automatically included to your Row as well. This also means when spec has changed, you don't have to edit all Entities. You only edit `StateMachine.IRow` and it will add all Rows that uses it.
```csharp
public sealed class CharacterRow : DescriptorRow<CharacterRow>,
    IQueryableRow<RageSet>,
    CharacterFSM.IRow
{ }
```

Now call `EnginesRoot.AddStateMachine` to add State Machine, along with your Schema.
```csharp
IndexedDB indexedDB = _enginesRoot.GeneratedIndexedDB();
GameSchema schema = _enginesRoot.AddSchema<GameSchema>(indexedDB);
CharacterFSM characterFSM = _enginesRoot.AddStateMachine<CharacterFSM>(indexedDB);
```

You can build entities as same and can set Initial State with it.
```csharp
var builder = _schema.Character.Build(_factory, entityID);
builder.Init(new CharacterFSM.Component(CharacterState.Normal));
```
But to make Transitions happen, **make sure you call `StateMachine.Engine.Step()`**. It is `IStepEngine` so you have option to pass it to `SortedEnginesGroup`, etc.

Lastly, you can query Entities by calling `Where()` with State Machine object. Same as you do with Indexes!
```csharp
characterFSM.Engine.Step();

foreach (var result in indexedDB.Select<RageSet>().From(schema.Character).Where(characterFSM.Is(CharacterState.Angry)).Entities())
{
    // ...
}
```

### Summary
We learned how to define and query with State Machine, and how to configure States, Transitions, Conditions, Callbacks. You've went through all the basic features of Schema extensions, hooray! You can look through additional documents or go ahead and try yourself!
