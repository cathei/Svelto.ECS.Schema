using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class RowQueryGenerator : ISourceGenerator
    {
        const string RowQueryTemplate = @"
        // Select -> Groups
        public static LocalFasterReadOnlyList<ExclusiveGroupStruct> Groups<{1}>(
                this (IndexedDB, IEntityRow<{1}>) query)
{2}
        {{
            // TODO this ultimately should return Tables<TR>
            return query.Item1.entitiesDB.FindGroups<{1}>();
        }}

        // Select -> Entities
        public static GroupsEnumerable<{1}> Entities<{1}>(
                this (IndexedDB, IEntityRow<{1}>) query)
{2}
        {{
            // TODO : is it possible to have findgroups cache inside of IndexedDB?
            // Is it better to cache tables or just rely on FindGroups?
            var groups = query.Item1.entitiesDB.FindGroups<{1}>();
            return query.Item1.entitiesDB.QueryEntities<{1}>(groups);
        }}

        // Select -> From Table -> Entities
        public static EntityCollection<{1}> Entities<{1}>(
                this (IndexedDB, IEntityRow<{1}>, IEntityTable) query)
{2}
        {{
            return query.Item1.entitiesDB.QueryEntities<{1}>(query.Item3.ExclusiveGroup);
        }}

        // Select -> From Tables -> Entities
        public static GroupsEnumerable<{1}> Entities<{1}>(
                this (IndexedDB, IEntityRow<{1}>, IEntityTables) query)
{2}
        {{
            return query.Item1.entitiesDB.QueryEntities<{1}>(query.Item3.ExclusiveGroups);
        }}

        // Select -> Where -> Entities
        public static IndexQueryEnumerable<{1}> Entities<{1}, TI>(
                this (IndexedDB, IEntityRow<{1}>, TI) query)
{2}
            where TI : struct, IIndexQuery
        {{
            return new IndexQueryEnumerable<{1}>(
                query.Item1, query.Item3.GetIndexedKeyData(query.Item1).groups);
        }}

        // Select -> From Table -> Where -> Entities
        public static IndexQueryTuple<{1}> Entities<{1}, TI>(
                this (IndexedDB, IEntityRow<{1}>, IEntityTable, TI) query)
{2}
            where TI : struct, IIndexQuery
        {{
            return new IndexQueryTuple<{1}>(
                (query.Item1, query.Item2, query.Item3).Entities(), query.Indices());
        }}

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryGroupsEnumerable<{1}> Entities<{1}, TI>(
                this (IndexedDB, IEntityRow<{1}>, IEntityTables, TI) query)
{2}
            where TI : struct, IIndexQuery
        {{
            var groups = query.Item3.ExclusiveGroups;
            return new IndexQueryGroupsEnumerable<{1}>(
                query.Item1, query.Item4.GetIndexedKeyData(query.Item1).groups, groups);
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

namespace Svelto.ECS.Schema
{{
    public static partial class RowQueryExtensions
    {{
{GenerateQueryEntities(RowQueryTemplate)}
    }}
}}";

            context.AddSource("RowQuery.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
