using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class StateMachineTests : SchemaTestsBase<StateMachineTests.TestSchema>
    {
        public struct RageComponent : IEntityComponent
        {
            public int value = 0;
        }

        public struct TriggerComponent : IEntityComponent
        {
            public bool value = false;
        }

        public struct SpecialTimerComponent : IEntityComponent
        {
            public float timer = 0;
        }

        public enum CharacterState { Normal, Upset, Angry, Special }

        public class CharacterFSM : StateMachine<CharacterState>
        {
            public CharacterFSM()
            {
                AddState(CharacterState.Normal)
                    .AddTransition(CharacterState.Upset)
                        .AddCondition((ref RageComponent rage) => rage.value >= 10)
                    .AddTransition(CharacterState.Special)
                        .AddCondition((ref RageComponent rage) => rage.value < 0)
                        .AddCondition((ref TriggerComponent trigger) => trigger.value);

                AddState(CharacterState.Upset)
                    .AddTransition(CharacterState.Normal)
                        .AddCondition((ref RageComponent rage) => rage.value < 10)
                    .AddTransition(CharacterState.Angry)
                        .AddCondition((ref RageComponent rage) => rage.value >= 20);

                AddState(CharacterState.Angry)
                    .AddTransition(CharacterState.Normal)
                        .AddCondition((ref RageComponent rage) => rage.value < 20);

                AddState(CharacterState.Special)
                    .AddTransition(CharacterState.Normal)
                        .AddCondition((ref SpecialTimerComponent timer) => timer.timer > 1);
            }
        }

        public class CharacterDescriptor : GenericEntityDescriptor<
            RageComponent, TriggerComponent, SpecialTimerComponent, CharacterFSM.Component>
        {

        }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterDescriptor> Character = new Table<CharacterDescriptor>();
        }

    }
}
