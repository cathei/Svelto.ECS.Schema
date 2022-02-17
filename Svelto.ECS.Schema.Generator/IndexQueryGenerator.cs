using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryGenerator : ISourceGenerator
    {
        const string QueryEntitiesTemplate = @"
        public IndexQueryEnumerable<{1}> Entities<{1}>()
{2}
        {{
            return new IndexQueryEnumerable<{1}>(context, groupDataList);
        }}
";

        const string QueryEntitiesWithGroupTemplate = @"
        public ({0}, FilteredIndices) Entities<{1}>(in ExclusiveGroupStruct group)
{2}
        {{
            FilteredIndices indices = EmptyFilteredIndices;

            if (groupDataList != null &&
                groupDataList.TryGetValue(group, out var groupData) &&
                groupData.group.IsEnabled())
            {{
                indices = groupData.filter.filteredIndices;
            }}

            var ({3}, _) = context.entitiesDB.QueryEntities<{1}>(group);

            return ({3}, indices);
        }}
";

        const string QueryEntitiesWithGroupsTemplate = @"
        public IndexQueryGroupsEnumerable<{1}> Entities<{1}>(FasterList<ExclusiveGroupStruct> groups)
{2}
        {{
            return new IndexQueryGroupsEnumerable<{1}>(context, groupDataList, groups);
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
            var nativeBufferTypeList = "NB<T{0}>".Repeat(", ", num);
            var genericTypeList = "T{0}".Repeat(", ", num);
            var typeConstraintList = "                where T{0} : unmanaged, IEntityComponent".Repeat("\n", num);
            var componentList = "c{0}".Repeat(", ", num);

            var builder = new StringBuilder();
            builder.AppendFormat(template, nativeBufferTypeList, genericTypeList, typeConstraintList, componentList);
            return builder;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string source = $@" // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{{
    public partial class SchemaContext
    {{
        public partial struct QueryAccessor
        {{
{GenerateQueryEntities(QueryEntitiesTemplate)}
{GenerateQueryEntities(QueryEntitiesWithGroupTemplate)}
{GenerateQueryEntities(QueryEntitiesWithGroupsTemplate)}
        }}
    }}
}}";

            context.AddSource("IndexQuery.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
