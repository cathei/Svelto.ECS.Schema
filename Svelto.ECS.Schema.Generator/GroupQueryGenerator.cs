using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class GroupQueryGenerator : ISourceGenerator
    {
        const string QueryGroupTemplate = @"
        public EntityCollection<{0}> Entities<{0}>(EntitiesDB entitiesDB)
{1}
        {{
            return entitiesDB.QueryEntities<{0}>(exclusiveGroup);
        }}
";

        const string QueryGroupsTemplate = @"
        public GroupsEnumerable<{0}> Entities<{0}>(EntitiesDB entitiesDB)
{1}
        {{
            return entitiesDB.QueryEntities<{0}>(exclusiveGroups);
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
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{{
    public readonly partial struct Group<T>
    {{
{GenerateQueryEntities(QueryGroupTemplate)}
    }}

    public partial struct Groups<T>
    {{
{GenerateQueryEntities(QueryGroupsTemplate)}
    }}
}}";

            context.AddSource("Group.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
