using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryGenerator : ISourceGenerator
    {
        const string QueryEntitiesTemplate = @"
        public IndexQueryEnumerable<{1}> Entities<{1}>(SchemaContext context)
{2}
        {{
            return new IndexQueryEnumerable<{1}>(context, GetGroupIndexDataList(context));
        }}
";

        const string QueryEntitiesWithGroupTemplate = @"
        public ({0}, FilteredIndices) Entities<{1}>(SchemaContext context)
{2}
        {{
            var groupDataList = _query.GetGroupIndexDataList(context);

            var indices = new FilteredIndices();

            if (groupDataList != null && _group.IsEnabled() &&
                groupDataList.TryGetValue(_group, out var groupData))
            {{
                indices = groupData.filter.filteredIndices;
            }}

            var ({3}, _) = context.entitiesDB.QueryEntities<{1}>(_group);

            return ({3}, indices);
        }}
";

        const string QueryEntitiesWithGroupsTemplate = @"
        public IndexQueryGroupsEnumerable<{1}> Entities<{1}>(SchemaContext context)
{2}
        {{
            return new IndexQueryGroupsEnumerable<{1}>(context, _query.GetGroupIndexDataList(context), _groups);
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
    public partial struct IndexQuery<T>
    {{
{GenerateQueryEntities(QueryEntitiesTemplate)}

        public partial struct FromGroupAccessor<TDesc>
        {{
{GenerateQueryEntities(QueryEntitiesWithGroupTemplate)}
        }}

        public partial struct FromGroupsAccessor<TDesc>
        {{
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
