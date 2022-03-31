using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class MemoTests : SchemaTestsBase<MemoTests.TestSchema>
    {
        public enum CharacterState { Happy, Sad, Angry, MAX };

        public struct CharacterControllerComponent : IIndexableComponent<int>
        {
            public EGID ID { get; set; }
            public int key { get; set; }
        }

        public struct CharacterGroupComponent : IPrimaryKeyComponent<int>
        {
            public int key { get; set; }
        }

        public struct CharacterStateComponent : IIndexableComponent<EnumKey<CharacterState>>
        {
            public EGID ID { get; set; }
            public EnumKey<CharacterState> key { get; set; }
        }

        public struct ControllerAndStateSet : IResultSet<CharacterControllerComponent, CharacterStateComponent>
        {
            public int count { get; set; }

            public NB<CharacterControllerComponent> controller;
            public NB<CharacterStateComponent> state;

            public void Init(in EntityCollection<CharacterControllerComponent, CharacterStateComponent> buffers)
            {
                (controller, state, count) = buffers;
            }
        }

        public class CharacterRow :
            DescriptorRow<CharacterRow>,
            IPrimaryKeyRow<CharacterGroupComponent>,
            IIndexableRow<CharacterControllerComponent>,
            IIndexableRow<CharacterStateComponent>,
            IQueryableRow<ControllerAndStateSet>,
            IMemorableRow
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterRow> Character = new();

            public readonly PrimaryKey<CharacterGroupComponent> CharacterGroup = new();

            public readonly Index<CharacterControllerComponent> Controller = new();
            public readonly Index<CharacterStateComponent> State = new();

            public readonly Memo<CharacterRow> Memo = new();

            public TestSchema()
            {
                Character.AddPrimaryKey(CharacterGroup);
                CharacterGroup.SetPossibleKeys(Enumerable.Range(0, 5).ToArray());
            }
        }

        [Fact]
        public void MemoUnionTest()
        {
            for (int i = 0; i < 99; ++i)
            {
                var builder = _factory.Build(_schema.Character, (uint)i);

                builder.Init(new CharacterControllerComponent { key = i / 10 });
                builder.Init(new CharacterStateComponent { key = (CharacterState)(i % (int)CharacterState.MAX) });
                builder.Init(new CharacterGroupComponent { key = i % _schema.CharacterGroup.PossibleKeyCount });
            }

            _submissionScheduler.SubmitEntities();

            _indexedDB.Memo(_schema.Memo).Clear();

            // 30 entities
            _indexedDB.Memo(_schema.Memo)
                .Union(_schema.Controller.Is(0))
                .Union(_schema.Controller.Is(3))
                .Union(_schema.Controller.Is(6));

            // 33 entities
            _indexedDB.Memo(_schema.Memo)
                .Union(_schema.State.Is(CharacterState.Happy));

            int entityCount = 0;

            foreach (var query in _indexedDB.From(_schema.Character).Where(_schema.Memo))
            {
                query.Select(out ControllerAndStateSet result);
                
                foreach (int i in query.indices)
                {
                    bool controllerMatch =
                        result.controller[i].key == 0 ||
                        result.controller[i].key == 3 ||
                        result.controller[i].key == 6;

                    bool stateMatch = result.state[i].key == CharacterState.Happy;

                    Assert.True(controllerMatch || stateMatch);

                    ++entityCount;
                }
            }

            // total expected = 30 + 33 - 12 (intersection)
            Assert.Equal(30 + 33 - 12, entityCount);
        }

        [Fact]
        public void MemoIntersectTest()
        {
            for (int i = 0; i < 99; ++i)
            {
                var builder = _factory.Build(_schema.Character, (uint)i);

                builder.Init(new CharacterControllerComponent { key = i / 10 });
                builder.Init(new CharacterStateComponent { key = (CharacterState)(i % (int)CharacterState.MAX) });
                builder.Init(new CharacterGroupComponent { key = i % _schema.CharacterGroup.PossibleKeyCount });
            }

            _submissionScheduler.SubmitEntities();

            _indexedDB.Memo(_schema.Memo).Clear();

            // 30 entities
            _indexedDB.Memo(_schema.Memo)
                .Union(_schema.Controller.Is(0))
                .Union(_schema.Controller.Is(3))
                .Union(_schema.Controller.Is(6));

            // 33 entities
            _indexedDB.Memo(_schema.Memo)
                .Intersect(_schema.State.Is(CharacterState.Happy));

            int entityCount = 0;

            foreach (var query in _indexedDB.From(_schema.Character).Where(_schema.Memo))
            {
                query.Select(out ControllerAndStateSet result);

                foreach (int i in query.indices)
                {
                    bool controllerMatch =
                        result.controller[i].key == 0 ||
                        result.controller[i].key == 3 ||
                        result.controller[i].key == 6;

                    bool stateMatch = result.state[i].key == CharacterState.Happy;

                    Assert.True(controllerMatch && stateMatch);

                    ++entityCount;
                }
            }

            // total expected = 12
            Assert.Equal(12, entityCount);
        }
    }
}
