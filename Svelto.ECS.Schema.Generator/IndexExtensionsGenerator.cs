using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class IndexExtensionsGenerator : ISourceGenerator
    {
        const string QueryEntitiesTemplate = @"
        public static IEnumerable<(({0}, FilteredIndices), ExclusiveGroupStruct)> QueryEntities<{1}>(this SchemaContext context, IndexQuery query)
{2}
        {{
            var groupDataList = context.GetGroupIndexDataList(query);

            if (groupDataList == null)
                yield break;

            var values = groupDataList.unsafeValues;

            for (int i = 0; i < groupDataList.count; ++i)
            {{
                var groupData = values[i];

                if (!groupData.group.IsEnabled())
                    continue;

                var ({3}, _) = context.entitiesDB.QueryEntities<{1}>(groupData.group);
                var indices = groupData.filter.filteredIndices;

                yield return (({3}, indices), groupData.group);
            }}
        }}

";

        const string QueryEntitiesWithGroupTemplate = @"
        public static ({0}, FilteredIndices) QueryEntities<{1}>(this SchemaContext context, IndexQuery query, in ExclusiveGroupStruct group)
{2}
        {{
            var groupDataList = context.GetGroupIndexDataList(query);

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
    public static partial class IndexExtensions
    {{
{GenerateQueryEntities(QueryEntitiesTemplate)}
{GenerateQueryEntities(QueryEntitiesWithGroupTemplate)}
    }}
}}";

            context.AddSource("IndexExtensions.g.cs", source);
        }


        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
