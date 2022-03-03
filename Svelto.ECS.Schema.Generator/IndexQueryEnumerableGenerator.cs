using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryEnumerableGenerator : ISourceGenerator
    {
        const string IndexQueryEnumerableTemplate = @"
    public readonly ref struct IndexQueryEnumerable<TR, TIR, {1}>
        where TR : IEntityRow<{1}>, TIR
        where TIR : IEntityRow
{2}
    {{
        private readonly IndexedDB _indexedDB;
        private readonly IEntityTables<TR> _tables;
        private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

        internal IndexQueryEnumerable(IndexedDB indexedDB,
            IEntityTables<TR> tables,
            FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict)
        {{
            _indexedDB = indexedDB;
            _tables = tables;
            _dict = dict;
        }}

        public RefIterator GetEnumerator() => new RefIterator(_indexedDB, _tables, _dict);

        public ref struct RefIterator
        {{
            private readonly IndexedDB _indexedDB;
            private readonly IEntityTables<TR> _tables;
            private readonly FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> _dict;

            private EntityCollection<{1}> _collection;
            private FilteredIndices _indices;
            private int _indexValue;

            internal RefIterator(IndexedDB indexedDB,
                IEntityTables<TR> tables,
                FasterDictionary<ExclusiveGroupStruct, IndexedGroupData<TIR>> dict) : this()
            {{
                _indexedDB = indexedDB;
                _tables = tables;
                _dict = dict;
                _indexValue = -1;
            }}

            public bool MoveNext()
            {{
                if (_dict == null)
                    return false;

                while (++_indexValue < _tables.Range)
                {{
                    var table = _tables.GetTable(_indexValue);

                    if (!_dict.TryGetValue(table.ExclusiveGroup, out var groupData))
                        continue;

                    _indices = groupData.filter.filteredIndices;

                    if (!table.ExclusiveGroup.IsEnabled() || _indices.Count() == 0)
                        continue;

                    _collection = _indexedDB.Select<TR>().From(table).Entities();
                    break;
                }}

                var moveNext = _indexValue < _tables.Range;

                if (!moveNext)
                    Reset();

                return moveNext;
            }}

            public void Reset() {{ _indexValue = -1; }}

            public IndexQueryTableTuple<TR, {1}> Current => new IndexQueryTableTuple<TR, {1}>(
                _collection, new IndexedIndices(_indices), _tables.GetTable(_indexValue));
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
{Generate(IndexQueryEnumerableTemplate)}
}}";

            context.AddSource("IndexQueryEnumerable.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
