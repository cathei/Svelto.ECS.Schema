using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class TableQueryGenerator : ISourceGenerator
    {
        const string QueryTableTemplate = @"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityCollection<{0}> Entities<{0}>(EntitiesDB entitiesDB)
{1}
        {{
            return entitiesDB.QueryEntities<{0}>(_exclusiveGroup);
        }}
";

        const string QueryTablesTemplate = @"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupsEnumerable<{0}> Entities<{0}>(EntitiesDB entitiesDB)
{1}
        {{
            return entitiesDB.QueryEntities<{0}>(_groups);
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
            builder.AppendFormat(template, genericTypeList, typeConstraintList);
            return builder;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string source = $@" // Auto-generated code
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema.Definition
{{
    public partial class Table<T>
    {{
{GenerateQueryEntities(QueryTableTemplate)}
    }}
}}

namespace Svelto.ECS.Schema.Internal
{{
    public partial class TablesBase<TDesc>
    {{
{GenerateQueryEntities(QueryTablesTemplate)}
    }}
}}";

            context.AddSource("TableQuery.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
