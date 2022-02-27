using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryGenerator : ISourceGenerator
    {
        const string QueryEntitiesTemplate = @"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexQueryEnumerable<{1}> Entities<{1}>(IndexesDB indexesDB)
{2}
        {{
            return new IndexQueryEnumerable<{1}>(
                indexesDB, GetGroupIndexDataList(indexesDB).groups);
        }}
";

        const string QueryEntitiesWithGroupTemplate = @"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexQueryTuple<{1}> Entities<{1}>(IndexesDB indexesDB)
{2}
        {{
            return new IndexQueryTuple<{1}>(
                indexesDB.entitiesDB.QueryEntities<{1}>(_group), Indices(indexesDB));
        }}
";

        const string QueryEntitiesWithGroupsTemplate = @"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexQueryGroupsEnumerable<{1}> Entities<{1}>(IndexesDB indexesDB)
{2}
        {{
            return new IndexQueryGroupsEnumerable<{1}>(
                indexesDB, _query.GetGroupIndexDataList(indexesDB).groups, _groups);
        }}
";

        public string GenerateQueryEntities(string template)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i <= 4; ++i)
            {
                builder.Append(GenerateQueryEntities(template, i));
            }

            return builder.ToString();
        }

        public StringBuilder GenerateQueryEntities(string template, int num)
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
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema.Definition
{{
    public partial class Memo<T>
    {{
{GenerateQueryEntities(QueryEntitiesTemplate)}
    }}
}}

namespace Svelto.ECS.Schema
{{
    public partial struct IndexQuery<TK, TC>
    {{
{GenerateQueryEntities(QueryEntitiesTemplate)}
    }}

    public partial struct IndexGroupQuery<TQuery>
    {{
{GenerateQueryEntities(QueryEntitiesWithGroupTemplate)}
    }}

    public partial struct IndexGroupsQuery<TQuery>
    {{
{GenerateQueryEntities(QueryEntitiesWithGroupsTemplate)}
    }}
}}";

            context.AddSource("IndexQuery.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
