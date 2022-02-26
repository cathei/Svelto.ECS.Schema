using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    public class MemoTests : SchemaTestsBase<MemoTests.TestSchema>
    {
        public class CharacterController : IndexTag<int, CharacterController.Unique>
        {
            public struct Unique : IUnique {}
        }

        public class CharacterState : IndexTag<CharacterState.Type, CharacterState.Unique>
        {
            public enum Type { Happy, Sad, Angry, MAX }

            public struct Unique : IUnique {}
        }

        public class CharacterDescriptor : GenericEntityDescriptor<
            EGIDComponent, CharacterController.Component, CharacterState.Component> { }

        public class TestSchema : IEntitySchema
        {
            public readonly RangedTable<CharacterDescriptor> Characters = new RangedTable<CharacterDescriptor>(5);

            public readonly CharacterController.Index CharacterByController = new CharacterController.Index();
            public readonly CharacterState.Index CharacterByState = new CharacterState.Index();

            public readonly Memo<EGIDComponent> Memo = new Memo<EGIDComponent>();
        }

        [Fact]
        public void MemoUnionTest()
        {
            for (int i = 0; i < 99; ++i)
            {
                var builder = _schema.Characters[i % _schema.Characters.Range].Build(_factory, (uint)i);

                builder.Init(new CharacterController.Component(i / 10));
                builder.Init(new CharacterState.Component((CharacterState.Type)(i % (int)CharacterState.Type.MAX)));
            }

            _submissionScheduler.SubmitEntities();

            _schema.Memo.Clear(_indexesDB);

            // 30 entities
            _schema.CharacterByController.Query(0).Union(_indexesDB, _schema.Memo);
            _schema.CharacterByController.Query(3).Union(_indexesDB, _schema.Memo);
            _schema.CharacterByController.Query(6).Union(_indexesDB, _schema.Memo);

            // 33 entities
            _schema.CharacterByState.Query(CharacterState.Type.Happy).Union(_indexesDB, _schema.Memo);

            int entityCount = 0;

            foreach (var ((controller, state, indices), group) in
                _schema.Memo.Entities<CharacterController.Component, CharacterState.Component>(_indexesDB))
            {
                foreach (int i in indices)
                {
                    bool controllerMatch =
                        controller[i].Value == 0 ||
                        controller[i].Value == 3 ||
                        controller[i].Value == 6;

                    bool stateMatch =
                        state[i].Value == CharacterState.Type.Happy;

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

                builder.Init(new CharacterController.Component(i / 10));
                builder.Init(new CharacterState.Component((CharacterState.Type)(i % (int)CharacterState.Type.MAX)));
            }

            _submissionScheduler.SubmitEntities();

            _schema.Memo.Clear(_indexesDB);

            // 30 entities
            _schema.CharacterByController.Query(0).Union(_indexesDB, _schema.Memo);
            _schema.CharacterByController.Query(3).Union(_indexesDB, _schema.Memo);
            _schema.CharacterByController.Query(6).Union(_indexesDB, _schema.Memo);

            // 33 entities
            _schema.CharacterByState.Query(CharacterState.Type.Happy).Intersect(_indexesDB, _schema.Memo);

            int entityCount = 0;

            foreach (var ((controller, state, indices), group) in
                _schema.Memo.Entities<CharacterController.Component, CharacterState.Component>(_indexesDB))
            {
                foreach (int i in indices)
                {
                    bool controllerMatch =
                        controller[i].Value == 0 ||
                        controller[i].Value == 3 ||
                        controller[i].Value == 6;

                    bool stateMatch =
                        state[i].Value == CharacterState.Type.Happy;

                    Assert.True(controllerMatch && stateMatch);

                    ++entityCount;
                }
            }

            // total expected = 12
            Assert.Equal(12, entityCount);
        }
    }
}
