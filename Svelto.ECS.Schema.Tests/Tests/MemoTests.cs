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
        public class CharacterDescriptor : GenericEntityDescriptor<EGIDComponent> { }

        public class TestSchema : IEntitySchema
        {
            public readonly Table<CharacterDescriptor> Characters = new Table<CharacterDescriptor>();
        }

        [Fact]
        public void GroupNameTest()
        {

        }
    }
}
