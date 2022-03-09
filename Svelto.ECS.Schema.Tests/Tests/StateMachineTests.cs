using System;
using System.Diagnostics.CodeAnalysis;
using Svelto.DataStructures;
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

        public enum CharacterState { Normal, Upset, Angry, Special, MAX }

        public struct CharacterStateComponent : IStateMachineComponent<EnumKey<CharacterState>>
        {
            public EGID ID { get; set; }
            public EnumKey<CharacterState> key { get; set; }

            public CharacterStateComponent(CharacterState state) : this()
            {
                this.key = state;
            }
        }

        public struct RageResultSet : IResultSet<RageComponent, CharacterStateComponent>
        {
            public int count { get; set; }

            public NB<RageComponent> rage;
            public NB<CharacterStateComponent> state;

            public void Init(in EntityCollection<RageComponent, CharacterStateComponent> buffers)
            {
                (rage, state, count) = buffers;
            }
        }

        public struct AllFourSet : IResultSet<RageComponent, TriggerComponent, SpecialTimerComponent, CharacterStateComponent>
        {
            public int count { get; set; }

            public NB<RageComponent> rage;
            public NB<TriggerComponent> trigger;
            public NB<SpecialTimerComponent> timer;
            public NB<CharacterStateComponent> state;

            public void Init(in EntityCollection<RageComponent, TriggerComponent, SpecialTimerComponent, CharacterStateComponent> buffers)
            {
                (rage, trigger, timer, state, count) = buffers;
            }
        }

        public class CharacterFSM : StateMachine<CharacterStateComponent>
        {
            public interface IRow : IIndexableRow,
                IEntityRow<RageComponent>,
                IEntityRow<TriggerComponent>,
                IEntityRow<SpecialTimerComponent>
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
                    .AddCondition((ref CharacterStateComponent self) => self.key != CharacterState.Special)
                    .AddCondition((ref RageComponent rage) => rage.value < 10);
            }
        }

        public class CharacterRow : DescriptorRow<CharacterRow>,
            CharacterFSM.IRow, IQueryableRow<RageResultSet>, IQueryableRow<AllFourSet>
        { }

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
            var result = _indexedDB.Select<RageResultSet>().From(_schema.Character).Entities();

            int totalCheckedCount = 0;

            for (CharacterState state = 0; state < CharacterState.MAX; ++state)
            {
                var indices = _indexedDB.Select<RageResultSet>()
                    .From(_schema.Character).Where(_characterFSM.Is(state)).Indices();

                foreach (var i in indices)
                {
                    // component state must match
                    Assert.Equal(state, (CharacterState)result.set.state[i].key);

                    ++totalCheckedCount;
                }
            }

            // all components should belong in index
            Assert.Equal(result.set.count, totalCheckedCount);
        }

        [Fact]
        public void NormalToUpsetTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterStateComponent(CharacterState.Normal));
            }

            _submissionScheduler.SubmitEntities();

            _characterFSM.Engine.Step();

            AssertIndexer();

            var result = _indexedDB.Select<RageResultSet>().From(_schema.Character).Entities();

            foreach (var i in result.indices)
            {
                Assert.Equal(CharacterState.Normal, (CharacterState)result.set.state[i].key);
                result.set.rage[i].value = (int)(i * 2);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            for (int i = 0; i < result.set.count; ++i)
            {
                Assert.Equal(i < 5 ? CharacterState.Normal : CharacterState.Upset,
                    (CharacterState)result.set.state[i].key);
            }
        }

        [Fact]
        public void NormalToAngryTest()
        {
            for (uint i = 0; i < 10; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterStateComponent(CharacterState.Normal));
                character.Init(new RageComponent { value = 100 });
            }

            _submissionScheduler.SubmitEntities();

            var result = _indexedDB.Select<RageResultSet>().From(_schema.Character).Entities();

            AssertIndexer();

            foreach (var i in result.indices)
            {
                Assert.Equal(CharacterState.Normal, (CharacterState)result.set.state[i].key);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            foreach (var i in result.indices)
            {
                Assert.Equal(CharacterState.Upset, (CharacterState)result.set.state[i].key);
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            foreach (var i in result.indices)
            {
                Assert.Equal(CharacterState.Angry, (CharacterState)result.set.state[i].key);
            }
        }

        [Fact]
        public void NormalToSpecialTest()
        {
            for (uint i = 0; i < 100; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterStateComponent(CharacterState.Normal));
                character.Init(new RageComponent { value = -1 });
                character.Init(new TriggerComponent { value = i % 2 == 0 });
            }

            _submissionScheduler.SubmitEntities();

            var result = _indexedDB.Select<AllFourSet>().From(_schema.Character).Entities();

            _characterFSM.Engine.Step();

            AssertIndexer();

            foreach (var i in result.indices)
            {
                Assert.Equal(i % 2 == 0 ? CharacterState.Special : CharacterState.Normal, (CharacterState)result.set.state[i].key);

                // must assigned when ExecuteOnEnter
                Assert.False(result.set.trigger[i].value);
                Assert.Equal(i % 2 == 0 ? 1 : 0, result.set.timer[i].value);

                if (i % 3 == 0)
                    result.set.timer[i].value = 0;
            }

            _characterFSM.Engine.Step();

            AssertIndexer();

            foreach (var i in result.indices)
            {
                Assert.Equal(i % 3 != 0 && i % 2 == 0 ? CharacterState.Special : CharacterState.Normal, (CharacterState)result.set.state[i].key);

                // must assigned when ExecuteOnExit
                Assert.Equal(i % 3 == 0 && i % 2 == 0 ? 5 : -1, result.set.rage[i].value);
            }
        }

        [Fact]
        public void StateMachineMemoryTest()
        {
            for (uint i = 0; i < 1000; ++i)
            {
                var character = _factory.Build(_schema.Character, i);
                character.Init(new CharacterStateComponent(CharacterState.Normal));
            }

            _submissionScheduler.SubmitEntities();

            // warming up
            _characterFSM.Engine.Step();

            var result = _indexedDB.Select<RageResultSet>().From(_schema.Character).Entities();

            foreach (var i in result.indices)
            {
                Assert.Equal(CharacterState.Normal, (CharacterState)result.set.state[i].key);
                result.set.rage[i].value = (int)i * 2;
            }

            _characterFSM.Engine.Step();

            foreach (var i in result.indices)
            {
                result.set.rage[i].value = 0;
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
