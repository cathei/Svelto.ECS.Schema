using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class MemoTests : SchemaTestsBase<MemoTests.TestSchema>
    {
        public enum CharacterState { Happy, Sad, Angry, MAX };


        public struct CharacterController : IIndexKey<CharacterController>
        {
            private int key;

            public static implicit operator CharacterController(int key)
                => new CharacterController { key = key };

            public bool KeyEquals(in CharacterController other)
                => key.Equals(other.key);

            public int KeyHashCode() => key.GetHashCode();
        }

        public struct CharacterStateKey : IIndexKey<CharacterStateKey>
        {
            private CharacterState key;

            public static implicit operator CharacterStateKey(CharacterState key)
                => new CharacterStateKey { key = key };

            public bool KeyEquals(in CharacterStateKey other)
                => key.Equals(other.key);

            public int KeyHashCode() => key.GetHashCode();
        }


        public interface IIndexedController : IIndexedRow<CharacterController>
        { }

        public interface IIndexedState : IIndexedRow<CharacterStateKey>
        { }

        public interface IControllerStateRow : ISelectorRow<
            Indexed<CharacterController>, Indexed<CharacterStateKey>>
        { }

        public class CharacterRow : DescriptorRow<CharacterRow>,
            IMemorableRow,
            IIndexedController,
            IIndexedState,
            IControllerStateRow
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly Tables<CharacterRow> Characters = new Tables<CharacterRow>(5);

            public readonly Index<CharacterController> Controller = new Index<CharacterController>();
            public readonly Index<CharacterStateKey> State = new Index<CharacterStateKey>();

            public readonly Memo<CharacterRow> Memo = new Memo<CharacterRow>();
        }

        [Fact]
        public void MemoUnionTest()
        {
            for (int i = 0; i < 99; ++i)
            {
                var builder = _factory.Build(_schema.Characters[i % _schema.Characters.Range], (uint)i);

                builder.Init(new Indexed<CharacterController>(i / 10));
                builder.Init(new Indexed<CharacterStateKey>((CharacterState)(i % (int)CharacterState.MAX)));
            }

            _submissionScheduler.SubmitEntities();

            _indexedDB.Memo(_schema.Memo).Clear();

            // 30 entities
            _indexedDB.Memo(_schema.Memo).Union(_schema.Controller, 0);
            _indexedDB.Memo(_schema.Memo).Union(_schema.Controller, 3);
            _indexedDB.Memo(_schema.Memo).Union(_schema.Controller, 6);

            // 33 entities
            _indexedDB.Memo(_schema.Memo).Union(_schema.State, CharacterState.Happy);

            int entityCount = 0;

            foreach (var ((controller, state, _), indices, _) in _indexedDB
                .Select<IControllerStateRow>().From(_schema.Characters).Where(_schema.Memo).Entities())
            {
                foreach (int i in indices)
                {
                    bool controllerMatch =
                        controller[i].Key.KeyEquals(0) ||
                        controller[i].Key.KeyEquals(3) ||
                        controller[i].Key.KeyEquals(6);

                    bool stateMatch = state[i].Key.KeyEquals(CharacterState.Happy);

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
                var builder = _factory.Build(_schema.Characters[i % _schema.Characters.Range], (uint)i);

                builder.Init(new Indexed<CharacterController>(i / 10));
                builder.Init(new Indexed<CharacterStateKey>((CharacterState)(i % (int)CharacterState.MAX)));
            }

            _submissionScheduler.SubmitEntities();

            _indexedDB.Memo(_schema.Memo).Clear();

            // 30 entities
            _indexedDB.Memo(_schema.Memo).Union(_schema.Controller, 0);
            _indexedDB.Memo(_schema.Memo).Union(_schema.Controller, 3);
            _indexedDB.Memo(_schema.Memo).Union(_schema.Controller, 6);

            // 33 entities
            _indexedDB.Memo(_schema.Memo).Intersect(_schema.State, CharacterState.Happy);

            int entityCount = 0;

            foreach (var ((controller, state, _), indices, _) in _indexedDB
                .Select<IControllerStateRow>().From(_schema.Characters).Where(_schema.Memo).Entities())
            {
                foreach (int i in indices)
                {
                    bool controllerMatch =
                        controller[i].Key.KeyEquals(0) ||
                        controller[i].Key.KeyEquals(3) ||
                        controller[i].Key.KeyEquals(6);

                    bool stateMatch = state[i].Key.KeyEquals(CharacterState.Happy);

                    Assert.True(controllerMatch && stateMatch);

                    ++entityCount;
                }
            }

            // total expected = 12
            Assert.Equal(12, entityCount);
        }
    }
}
