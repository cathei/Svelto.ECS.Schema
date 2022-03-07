using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class RowQueryGenerator : ISourceGenerator
    {
        const string RowQueryTemplate = @"
        // Select -> From Table -> Entities
        public static QueryResult<TRS, TR> Entities<TR, TRS, {1}>(
                this (IndexedDB, IMagicTuple<IQueryableRowBase<IResultSet<{1}>>, TRS>, IEntityTable<TR>) query)
            where TR : class, IQueryableRow<TRS>
            where TRS : struct, IResultSet<{1}>
{2}
        {{
            TRS result = default;
            result.Init(query.Item1.entitiesDB.QueryEntities<{1}>(query.Item3.ExclusiveGroup));
            return new QueryResult<TRS, TR>(result, query.Item3);
        }}

        // Select -> From Tables -> Entities
        public static TablesEnumerable<TRS, TR, {1}> Entities<TR, TRS, {1}>(
                this (IndexedDB, IMagicTuple<IQueryableRowBase<IResultSet<{1}>>, TRS>, IEntityTables<TR>) query)
            where TR : class, IQueryableRow<TRS>
            where TRS : struct, IResultSet<{1}>
{2}
        {{
            return new TablesEnumerable<TRS, TR, {1}>(query.Item1, query.Item3);
        }}

        // Select -> From Table -> Where -> Entities
        public static IndexedQueryResult<TRS, TR> Entities<TR, TRS, TI, {1}>(
                this (IndexedDB, IMagicTuple<IQueryableRowBase<IResultSet<{1}>>, TRS>, IEntityTable<TR>, TI) query)
            where TR : class, IQueryableRow<TRS>
            where TRS : struct, IResultSet<{1}>
            where TI : IIndexQuery
{2}
        {{
            TRS result = default;
            result.Init(query.Item1.entitiesDB.QueryEntities<{1}>(query.Item3.ExclusiveGroup));
            return new IndexedQueryResult<TRS, TR>(result, query.Indices(), query.Item3);
        }}

        // Select -> From Tables -> Where -> Entities
        public static IndexQueryEnumerable<TRS, TR, {1}> Entities<TR, TRS, TI, {1}>(
                this (IndexedDB, IMagicTuple<IQueryableRowBase<IResultSet<{1}>>, TRS>, IEntityTables<TR>, TI) query)
            where TR : class, IQueryableRow<TRS>
            where TRS : struct, IResultSet<{1}>
            where TI : IIndexQuery
{2}
        {{
            return new IndexQueryEnumerable<TRS, TR, {1}>(
                query.Item1, query.Item3, query.Item4.GetIndexerKeyData(query.Item1).groups);
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
