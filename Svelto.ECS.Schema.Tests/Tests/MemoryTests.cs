using System;
using Svelto.ECS.Schema.Definition;
using Xunit;

namespace Svelto.ECS.Schema.Tests
{
    // not sure if this is correct way of checking zero-allocation
    // due to Buffer<T> boxing inside of Svelto I have to have some margin here
    // just to make sure it does not take excessive memory
    public class MemoryTests : SchemaTestsBase<MemoryTests.TestSchema>
    {
        public interface IHaveEGID : IEntityRow<EGIDComponent> { }

        public interface IIndexdItemOwner : IIndexedRow<int, IIndexdItemOwner.Tag>
        { public struct Tag : ITag { } }

        public class CharacterRow : DescriptorRow<CharacterRow>, IHaveEGID { }
        public class ItemRow : DescriptorRow<ItemRow>, IHaveEGID, IIndexdItemOwner { }

        public class TestSchema : IEntitySchema
        {
            public readonly CharacterRow.Table Character = new CharacterRow.Table();
            public readonly ItemRow.Tables Items = new ItemRow.Tables(10);

            public readonly IIndexdItemOwner.Index ItemOwner = new IIndexdItemOwner.Index();
        }

        public MemoryTests() : base()
        {
            for (int i = 0; i < 100; ++i)
            {
                _factory.Build(_schema.Character, (uint)i);

                for (int j = 0; j < 1000; ++j)
                {
                    var itemBuilder = _factory.Build(_schema.Items[j / 100], (uint)((i * 1000) + j));
                    itemBuilder.Init(new IIndexdItemOwner.Component(i));
                }
            }

            _submissionScheduler.SubmitEntities();
        }

        [Fact]
        public void GroupEntitiesTest()
        {
            // warming up
            var (egid, count) = _indexedDB.Select<IHaveEGID>().From(_schema.Character).Entities();
            var (indexed, count2) = _indexedDB.Select<IIndexdItemOwner>().From(_schema.Items[0]).Entities();

            long before = GC.GetAllocatedBytesForCurrentThread();

            (egid, count) = _indexedDB.Select<IHaveEGID>().From(_schema.Character).Entities();
            (indexed, count2) = _indexedDB.Select<IIndexdItemOwner>().From(_schema.Items[0]).Entities();

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());
        }

        [Fact]
        public void GroupsEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, count), table) in
                _indexedDB.Select<IIndexdItemOwner>().From(_schema.Items).Entities())
            {
                ++loop;

                Assert.Equal(10000, count);
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, count), table) in
                _indexedDB.Select<IIndexdItemOwner>().From(_schema.Items).Entities())
            {
                ++loop;
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
       }

        [Fact]
        public void IndexEntitiesTest()
        {
            int loop = 0;

            // warming up
            foreach (var ((indexed, count), indices, group) in _indexedDB
                .Select<IIndexdItemOwner>().All().Where(_schema.ItemOwner, 0).Entities())
            {
                ++loop;

                Assert.Equal(100, indices.Count());
            }

            Assert.True(loop > 0);

            loop = 0;

            long before = GC.GetAllocatedBytesForCurrentThread();

            foreach (var ((indexed, count), indices, group) in _indexedDB
                .Select<IIndexdItemOwner>().All().Where(_schema.ItemOwner, 0).Entities())
            {
                ++loop;
            }

            Assert.True(before + 50 > GC.GetAllocatedBytesForCurrentThread());

            Assert.True(loop > 0);
        }
    }
}
