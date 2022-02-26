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
                var stateNormal = AddState(CharacterState.Normal);
                var stateUpset = AddState(CharacterState.Upset);
                var stateAngry = AddState(CharacterState.Angry);
                var stateSpecial = AddState(CharacterState.Special);

                stateNormal.AddTransition(stateUpset)
                    .AddCondition((ref RageComponent rage) => rage.value >= 10);

                stateNormal.AddTransition(stateSpecial)
                    .AddCondition((ref RageComponent rage) => rage.value < 0)
                    .AddCondition((ref TriggerComponent trigger) => trigger.value);

                stateUpset.AddTransition(stateAngry)
                    .AddCondition((ref RageComponent rage) => rage.value >= 20);

                stateSpecial.AddTransition(stateNormal)
                    .AddCondition((ref SpecialTimerComponent timer) => timer.timer > 1);

                stateSpecial
                    .ExecuteOnEnter((ref TriggerComponent trigger) => trigger.value = false)
                    .ExecuteOnExit((ref RageComponent rage) => rage.value = 5);

                // any state but special
                AnyState.AddTransition(stateNormal)
                    .AddCondition((ref Component self) => self.State != CharacterState.Special)
                    .AddCondition((ref RageComponent rage) => rage.value < 10);
            }
        }

        public class CharacterDescriptor : GenericEntityDescriptor<
            RageComponent, TriggerComponent, SpecialTimerComponent, CharacterFSM.Component> { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterDescriptor> Character = new Table<CharacterDescriptor>();
        }

        private IStepEngine _characterFSMEngine;

        public StateMachineTests() : base()
        {
            _characterFSMEngine = _enginesRoot.AddStateMachine<CharacterFSM>(_indexesDB);
        }

        [Fact]
        public void NormalToUpsetTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var character = _schema.Character.Build(_factory, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
            }

            _submissionScheduler.SubmitEntities();

            _characterFSMEngine.Step();

            var (rage, fsm, count) = _schema.Character.Entities<RageComponent, CharacterFSM.Component>(_entitiesDB);

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].State);
                rage[i].value = i * 2;
            }

            _characterFSMEngine.Step();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(i < 5 ? CharacterState.Normal : CharacterState.Upset, fsm[i].State);
            }
        }
    }
}
