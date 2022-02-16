using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexQueryGenerator : ISourceGenerator
    {
        const string QueryEntitiesTemplate = @"
        public IEnumerable<(({0}, FilteredIndices), ExclusiveGroupStruct)> Entities<{1}>()
{2}
        {{
            if (groupDataList == null)
                yield break;

            var values = groupDataList.unsafeValues;

            for (int i = 0; i < groupDataList.count; ++i)
            {{
                var groupData = values[i];
                var indices = groupData.filter.filteredIndices;

                if (!groupData.group.IsEnabled() || indices.Count() == 0)
                    continue;

                var ({3}, _) = context.entitiesDB.QueryEntities<{1}>(groupData.group);

                yield return (({3}, indices), groupData.group);
            }}
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
        public IEnumerable<(({0}, FilteredIndices), ExclusiveGroupStruct)> Entities<{1}>(FasterList<ExclusiveGroupStruct> groups)
{2}
        {{
            if (groupDataList == null)
                yield break;

            for (int i = 0; i < groups.count; ++i)
            {{
                if (!groupDataList.TryGetValue(groups[i], out var groupData))
                    continue;

                var indices = groupData.filter.filteredIndices;

                if (!groupData.group.IsEnabled() || indices.Count() == 0)
                    continue;

                var ({3}, _) = context.entitiesDB.QueryEntities<{1}>(groupData.group);

                yield return (({3}, indices), groupData.group);
            }}
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
            var nativeBufferTypeList = "NB<T{0}>".Join(", ", num);
            var genericTypeList = "T{0}".Join(", ", num);
            var typeConstraintList = "                where T{0} : unmanaged, IEntityComponent".Join("\n", num);
            var componentList = "c{0}".Join(", ", num);

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
