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
            return new IndexQueryEnumerable<{1}>(indexesDB, GetGroupIndexDataList(indexesDB));
        }}
";

        const string QueryEntitiesWithGroupTemplate = @"
        public ({0}, IndexedIndices) Entities<{1}>(IndexesDB indexesDB)
{2}
        {{
            var groupDataList = _query.GetGroupIndexDataList(indexesDB);

            var indices = new FilteredIndices();

            if (groupDataList != null && _group.ExclusiveGroupStruct.IsEnabled() &&
                groupDataList.TryGetValue(_group, out var groupData))
            {{
                indices = groupData.filter.filteredIndices;
            }}

            var ({3}, _) = indexesDB.entitiesDB.QueryEntities<{1}>(_group);

            return ({3}, new IndexedIndices(indices));
        }}
";

        const string QueryEntitiesWithGroupsTemplate = @"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexQueryGroupsEnumerable<{1}> Entities<{1}>(IndexesDB indexesDB)
{2}
        {{
            return new IndexQueryGroupsEnumerable<{1}>(indexesDB, _query.GetGroupIndexDataList(indexesDB), _groups);
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
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Schema
{{
    public partial struct IndexQuery<TKey>
    {{
{GenerateQueryEntities(QueryEntitiesTemplate)}
    }}

    public partial struct IndexGroupQuery<TKey, TDesc>
    {{
{GenerateQueryEntities(QueryEntitiesWithGroupTemplate)}
    }}

    public partial struct IndexGroupsQuery<TKey, TDesc>
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
