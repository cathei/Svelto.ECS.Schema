using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class TablesEnumerableGenerator : ISourceGenerator
    {
        const string TablesEnumerableTemplate = @"
    public readonly ref struct TablesEnumerable<TResult, TRow, {1}>
        where TResult : struct, IResultSet<{1}>
        where TRow : class, IQueryableRow<TResult>
{2}
    {{
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TRow> _tables;

        internal TablesEnumerable(IndexedDB indexedDB, in IEntityTables<TRow> tables)
        {{
            _indexedDB = indexedDB;
            _tables = tables;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables);

        public ref struct RefIterator
        {{
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TRow> _tables;

            private TResult _result;
            private IEntityTable<TRow> _table;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB, in IEntityTables<TRow> tables) : this()
            {{
                _indexedDB = indexedDB;
                _tables = tables;
                _indexValue = -1;
            }}

            public bool MoveNext()
            {{
                while (++_indexValue < _tables.Range)
                {{
                    _table = _tables.GetTable(_indexValue);

                    if (!_table.ExclusiveGroup.IsEnabled())
                        continue;

                    var collection = _indexedDB.entitiesDB.QueryEntities<{1}>(_table.ExclusiveGroup);

                    // cannot do this due because count is internal...
                    // if (collection.count == 0)
                    //     continue;

                    _result.Init(collection);
                    break;
                }}

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }}

            public void Reset() {{ _indexValue = -1; _result = default; }}

            public void Dispose() {{ }}

            public QueryResult<TResult, TRow> Current =>
                new QueryResult<TResult, TRow>(_result, _table);
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
