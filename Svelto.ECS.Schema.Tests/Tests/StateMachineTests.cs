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
            public int value;
        }

        public struct TriggerComponent : IEntityComponent
        {
            public bool value;
        }

        public struct SpecialTimerComponent : IEntityComponent
        {
            public float value;
        }

        public interface ICharacterRow :
            IEntityRow<RageComponent, TriggerComponent, SpecialTimerComponent>,
            CharacterFSM.IRow
        { }

        public enum CharacterState { Normal, Upset, Angry, Special, MAX }

        public class CharacterFSM : StateMachine<ICharacterRow, CharacterFSM.Tag, CharacterState>
        {
            public struct Tag : ITag {}

            protected override void Configure()
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
                    .AddCondition((ref SpecialTimerComponent timer) => timer.value <= 0);

                stateSpecial
                    .ExecuteOnEnter((ref TriggerComponent trigger) => trigger.value = false)
                    .ExecuteOnEnter((ref SpecialTimerComponent timer) => timer.value = 1)
                    .ExecuteOnExit((ref RageComponent rage) => rage.value = 5);

                // any state but special
                AnyState.AddTransition(stateNormal)
                    .AddCondition((ref Component self) => self.State != CharacterState.Special)
                    .AddCondition((ref RageComponent rage) => rage.value < 10);
            }
        }

        public class CharacterRow : DescriptorRow<CharacterRow>, ICharacterRow { }

        public class TestSchema : IEntitySchema
        {
            public readonly CharacterRow.Table Character = new CharacterRow.Table();
        }

        private readonly CharacterFSM _characterFSM;

        public StateMachineTests() : base()
        {
            _characterFSM = _enginesRoot.AddStateMachine<CharacterFSM>(_indexedDB);
        }

        private void AssertIndexer()
        {
            var (component, count) = _indexedDB.Select<CharacterFSM.IRow>().From(_schema.Character).Entities();

            int totalCheckedCount = 0;

            for (CharacterState state = 0; state < CharacterState.MAX; ++state)
            {
                var indices = _characterFSM.Where(state).From(_schema.Character).Indices(_indexedDB);

                foreach (var i in indices)
                {
                    // component state must match
                    Assert.Equal(state, component[i].State);

                    ++totalCheckedCount;
                }
            }

            // all components should belong in index
            Assert.Equal(count, totalCheckedCount);
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

            _characterFSM.Engine.Step();

            AssertIndexer();

            var (rage, fsm, count) = _schema.Character.Entities<RageComponent, CharacterFSM.Component>(_entitiesDB);

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].State);
                rage[i].value = i * 2;
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(i < 5 ? CharacterState.Normal : CharacterState.Upset, fsm[i].State);
            }
        }

        [Fact]
        public void NormalToAngryTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var character = _schema.Character.Build(_factory, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
                character.Init(new RageComponent { value = 100 });
            }

            _submissionScheduler.SubmitEntities();

            var (rage, fsm, count) = _schema.Character.Entities<RageComponent, CharacterFSM.Component>(_entitiesDB);

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].State);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Upset, fsm[i].State);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Angry, fsm[i].State);
            }
        }

        [Fact]
        public void NormalToSpecialTest()
        {
            for (uint i = 0; i < 100; ++i)
            {
                var character = _schema.Character.Build(_factory, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
                character.Init(new RageComponent { value = -1 });
                character.Init(new TriggerComponent { value = i % 2 == 0 });
            }

            _submissionScheduler.SubmitEntities();

            var (rage, trigger, timer, fsm, count) = _schema.Character.Entities<
                RageComponent, TriggerComponent, SpecialTimerComponent, CharacterFSM.Component>(_entitiesDB);

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(i % 2 == 0 ? CharacterState.Special : CharacterState.Normal, fsm[i].State);

                // must assigned when ExecuteOnEnter
                Assert.False(trigger[i].value);
                Assert.Equal(i % 2 == 0 ? 1 : 0, timer[i].value);

                if (i % 3 == 0)
                    timer[i].value = 0;
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(i % 3 != 0 && i % 2 == 0 ? CharacterState.Special : CharacterState.Normal, fsm[i].State);

                // must assigned when ExecuteOnExit
                Assert.Equal(i % 3 == 0 && i % 2 == 0 ? 5 : -1, rage[i].value);
            }
        }

        [Fact]
        public void StateMachineMemoryTest()
        {
            for (uint i = 0; i < 1000; ++i)
            {
                var character = _schema.Character.Build(_factory, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
            }

            _submissionScheduler.SubmitEntities();

            // warming up
            _characterFSM.Engine.Step();

            var (rage, fsm, count) = _schema.Character.Entities<RageComponent, CharacterFSM.Component>(_entitiesDB);

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].State);
                rage[i].value = i * 2;
            }

            _characterFSM.Engine.Step();

            for (int i = 0; i < count; ++i)
            {
                rage[i].value = 0;
            }

            long before = GC.GetAllocatedBytesForCurrentThread();

            _characterFSM.Engine.Step();

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }
    }
}
