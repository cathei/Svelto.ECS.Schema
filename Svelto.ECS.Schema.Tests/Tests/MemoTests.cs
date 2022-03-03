using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class MemoTests : SchemaTestsBase<MemoTests.TestSchema>
    {
        public enum CharacterState { Happy, Sad, Angry, MAX };

        public interface IIndexedController : IIndexedRow<int, IIndexedController.Tag>
        { public struct Tag : ITag {} }

        public interface IIndexedState : IIndexedRow<CharacterState, IIndexedState.Tag>
        { public struct Tag : ITag {} }

        public interface IControllerStateRow : IEntityRow<
            IIndexedController.Component, IIndexedState.Component>
        { }

        public class CharacterRow : DescriptorRow<CharacterRow>,
            IMemorableRow,
            IIndexedController,
            IIndexedState,
            IControllerStateRow
        { }

        public class TestSchema : IEntitySchema
        {
            public readonly CharacterRow.Tables Characters = new CharacterRow.Tables(5);

            public readonly IIndexedController.Index Controller = new IIndexedController.Index();
            public readonly IIndexedState.Index State = new IIndexedState.Index();

            public readonly Memo<CharacterRow> Memo = new Memo<CharacterRow>();
        }

        [Fact]
        public void MemoUnionTest()
        {
            for (int i = 0; i < 99; ++i)
            {
                var builder = _schema.Characters[i % _schema.Characters.Range].Build(_factory, (uint)i);

                builder.Init(new IIndexedController.Component(i / 10));
                builder.Init(new IIndexedState.Component((CharacterState)(i % (int)CharacterState.MAX)));
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
                        controller[i].Value == 0 ||
                        controller[i].Value == 3 ||
                        controller[i].Value == 6;

                    bool stateMatch = state[i].Value == CharacterState.Happy;

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
                var builder = _schema.Characters[i % _schema.Characters.Range].Build(_factory, (uint)i);

                builder.Init(new IIndexedController.Component(i / 10));
                builder.Init(new IIndexedState.Component((CharacterState)(i % (int)CharacterState.MAX)));
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
                        controller[i].Value == 0 ||
                        controller[i].Value == 3 ||
                        controller[i].Value == 6;

                    bool stateMatch = state[i].Value == CharacterState.Happy;

                    Assert.True(controllerMatch && stateMatch);

                    ++entityCount;
                }
            }

            // total expected = 12
            Assert.Equal(12, entityCount);
        }
    }
}
