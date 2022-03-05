using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryTupleGenerator : ISourceGenerator
    {
        const string IndexQueryTupleTemplate = @"
    public readonly ref struct IndexQueryTuple<{1}, TExtra>
{2}
    {{
        private readonly EntityCollection<{1}> _collection;
        private readonly TExtra _extra;

        public IndexQueryTuple(in EntityCollection<{1}> collection, in TExtra extra)
        {{
            _collection = collection;
            _extra = extra;
        }}

        public void Deconstruct(out EntityCollection<{1}> collection, out TExtra extra)
        {{
            collection = _collection;
            extra = _extra;
        }}
    }}
";

        const string IndexQueryGroupTupleTemplate = @"
    public readonly ref struct IndexQueryTableTuple<TR, {1}>
        where TR : class, IEntityRow
{2}
    {{
        private readonly EntityCollection<{1}> _collection;
        private readonly IndexedIndices _indices;
        private readonly IEntityTable<TR> _table;

        public IndexQueryTableTuple(in EntityCollection<{1}> collection, in IndexedIndices indices, IEntityTable<TR> table)
        {{
            _collection = collection;
            _indices = indices;
            _table = table;
        }}

        public void Deconstruct(out EntityCollection<{1}> collection, out IndexedIndices indices, out IEntityTable<TR> table)
        {{
            collection = _collection;
            indices = _indices;
            table = _table;
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

namespace Svelto.ECS.Schema.Internal
{{
{Generate(IndexQueryTupleTemplate)}
{Generate(IndexQueryGroupTupleTemplate)}
}}";

            context.AddSource("IndexQueryTuple.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
