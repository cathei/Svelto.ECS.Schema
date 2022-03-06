using System;
using System.Diagnostics.CodeAnalysis;
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

        public interface IRageCharacterRow : ISelectorRow<RageComponent, CharacterFSM.Component>
        { }

        public interface IAllFourRow : ISelectorRow<RageComponent, TriggerComponent, SpecialTimerComponent, CharacterFSM.Component>
        { }

        public interface ICharacterRow : CharacterFSM.IRow, IRageCharacterRow, IAllFourRow
        { }

        public enum CharacterState { Normal, Upset, Angry, Special, MAX }

        public readonly struct CharacterFSMState : IStateMachineKey<CharacterFSMState>
        {
            private readonly CharacterState state;

            public CharacterFSMState(CharacterState state) { this.state = state; }

            public static implicit operator CharacterFSMState(CharacterState state) => new CharacterFSMState(state);

            public bool KeyEquals(in CharacterFSMState other) => state == other.state;

            public override bool Equals(object obj) => obj is CharacterFSMState other && KeyEquals(other);

            public int KeyHashCode() => state.GetHashCode();

            public override int GetHashCode() => state.GetHashCode();

            public override string ToString() => state.ToString();
        }

        public class CharacterFSM : StateMachine<CharacterFSMState>
        {
            public interface IRow : IIndexedRow,
                ISelectorRow<RageComponent>,
                ISelectorRow<TriggerComponent>,
                ISelectorRow<SpecialTimerComponent>
            { }

            protected override void OnConfigure()
            {
                var builder = Configure<IRow>();

                var stateNormal = builder.AddState(CharacterState.Normal);
                var stateUpset = builder.AddState(CharacterState.Upset);
                var stateAngry = builder.AddState(CharacterState.Angry);
                var stateSpecial = builder.AddState(CharacterState.Special);

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
                builder.AnyState.AddTransition(stateNormal)
                    .AddCondition((ref Component self) => !self.Key.KeyEquals(CharacterState.Special))
                    .AddCondition((ref RageComponent rage) => rage.value < 10);
            }
        }

        public class CharacterRow : DescriptorRow<CharacterRow>, ICharacterRow { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new Table<CharacterRow>();
        }

        private readonly CharacterFSM _characterFSM;

        public StateMachineTests() : base()
        {
            _characterFSM = _enginesRoot.AddStateMachine<CharacterFSM>(_indexedDB);
        }

        private void AssertIndexer()
        {
            var (component, count) = _indexedDB.Select<CharacterFSM.IIndexedRow>().From(_schema.Character).Entities();

            int totalCheckedCount = 0;

            for (CharacterState state = 0; state < CharacterState.MAX; ++state)
            {
                var indices = _indexedDB.Select<CharacterFSM.IIndexedRow>()
                    .From(_schema.Character).Where(_characterFSM, state).Indices();

                foreach (var i in indices)
                {
                    // component state must match
                    Assert.Equal(state, component[i].Key);

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
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
            }

            _submissionScheduler.SubmitEntities();

            _characterFSM.Engine.Step();

            AssertIndexer();

            var (rage, fsm, count) = _indexedDB.Select<IRageCharacterRow>().From(_schema.Character).Entities();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].Key);
                rage[i].value = i * 2;
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(i < 5 ? CharacterState.Normal : CharacterState.Upset, fsm[i].Key);
            }
        }

        [Fact]
        public void NormalToAngryTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
                character.Init(new RageComponent { value = 100 });
            }

            _submissionScheduler.SubmitEntities();

            var (rage, fsm, count) = _indexedDB.Select<IRageCharacterRow>().From(_schema.Character).Entities();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].Key);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Upset, fsm[i].Key);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Angry, fsm[i].Key);
            }
        }

        [Fact]
        public void NormalToSpecialTest()
        {
            for (uint i = 0; i < 100; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
                character.Init(new RageComponent { value = -1 });
                character.Init(new TriggerComponent { value = i % 2 == 0 });
            }

            _submissionScheduler.SubmitEntities();

            var (rage, trigger, timer, fsm, count) = _indexedDB
                .Select<IAllFourRow>().From(_schema.Character).Entities();

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(i % 2 == 0 ? CharacterState.Special : CharacterState.Normal, fsm[i].Key);

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
                Assert.Equal(i % 3 != 0 && i % 2 == 0 ? CharacterState.Special : CharacterState.Normal, fsm[i].Key);

                // must assigned when ExecuteOnExit
                Assert.Equal(i % 3 == 0 && i % 2 == 0 ? 5 : -1, rage[i].value);
            }
        }

        [Fact]
        public void StateMachineMemoryTest()
        {
            for (uint i = 0; i < 1000; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterFSM.Component(CharacterState.Normal));
            }

            _submissionScheduler.SubmitEntities();

            // warming up
            _characterFSM.Engine.Step();

            var (rage, fsm, count) = _indexedDB.Select<IRageCharacterRow>().From(_schema.Character).Entities();

            for (int i = 0; i < count; ++i)
            {
                Assert.Equal(CharacterState.Normal, fsm[i].Key);
                rage[i].value = i * 2;
            }

            _characterFSM.Engine.Step();

            for (int i = 0; i < count; ++i)
            {
                rage[i].value = 0;
            }

            _characterFSM.Engine.Step();

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 100; ++i)
            {
                _characterFSM.Engine.Step();
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }
    }
}
