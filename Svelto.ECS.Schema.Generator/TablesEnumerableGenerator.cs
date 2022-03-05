using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class TablesEnumerableGenerator : ISourceGenerator
    {
        const string TablesEnumerableTemplate = @"
    public readonly ref struct TablesEnumerable<TR, {1}>
        where TR : class, ISelectorRow<{1}>
{2}
    {{
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;

        internal TablesEnumerable(IndexedDB indexedDB, in IEntityTables<TR> tables)
        {{
            _indexedDB = indexedDB;
            _tables = tables;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {{
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;

            private EntityCollection<{1}> _collection;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TR> tables) : this()
            {{
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }}

            public bool MoveNext()
            {{
                while (++_indexValue < _tables.Range)
                {{
                    var group = _tables.GetTable(_indexValue).ExclusiveGroup;

                    if (!group.IsEnabled())
                        continue;

                    var collection = _indexedDB.entitiesDB.QueryEntities<{1}>(group);

                    // cannot do this due because count is internal...
                    // if (collection.count == 0)
                    //     continue;

                    _collection = collection;
                    break;
                }}

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }}

            public void Reset() {{ _indexValue = -1; }}

            public void Dispose() {{ }}

            public RefCurrent Current => new RefCurrent(_collection, _tables.GetTable(_indexValue));

            public readonly ref struct RefCurrent
            {{
                public readonly EntityCollection<{1}> _buffers;
                public readonly IEntityTable<TR> _table;

                public RefCurrent(in EntityCollection<{1}> buffers, IEntityTable<TR> table)
                {{
                    _buffers = buffers;
                    _table = table;
                }}

                public void Deconstruct(out EntityCollection<{1}> buffers, out IEntityTable<TR> table)
                {{
                    buffers = _buffers;
                    table = _table;
                }}
            }}
        }}
    }}
";

        public string Generate(string template)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i <= 4; ++i)
            {
                builder.Append(Generate(template, i));
            }

            return builder.ToString();
        }

        public StringBuilder Generate(string template, int num)
        {
            var genericTypeList = "T{0}".Repeat(", ", num);
            var typeConstraintList = "                where T{0} : struct, IEntityComponent".Repeat("\n", num);

            var builder = new StringBuilder();
            builder.AppendFormat(template, "unused", genericTypeList, typeConstraintList);
            return builder;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string source = $@" // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{{
{Generate(TablesEnumerableTemplate)}
}}";

            context.AddSource("TablesEnumerable.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
