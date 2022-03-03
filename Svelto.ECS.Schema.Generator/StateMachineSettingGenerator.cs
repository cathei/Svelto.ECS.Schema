using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    [Generator]
    public class StageMachineConfigGenerator : ISourceGenerator
    {
        const string StateMachineConfigNativeTemplate = @"
        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, {1}>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateNative<{3}> preciate)
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TS : unmanaged
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<{1}>
{2}
        {{
            var condition = new StateMachine<TS, TT>.ConditionConfigNative<{3}>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }}

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, {1}>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<{3}> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<{1}>
{2}
        {{
            var config = new StateMachine<TS, TT>.CallbackConfigNative<{3}>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }}

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, {1}>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackNative<{3}> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<{1}>
{2}
        {{
            var config = new StateMachine<TS, TT>.CallbackConfigNative<{3}>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }}
";

        const string StateMachineConfigManagedTemplate = @"
        public static StateMachine<TS, TT>.Builder<TR>.TransitionBuilder AddCondition<TS, TT, TR, {1}>(
                this StateMachine<TS, TT>.Builder<TR>.TransitionBuilder builder,
                PredicateManaged<{3}> preciate)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<{1}>
{2}
        {{
            var condition = new StateMachine<TS, TT>.ConditionConfigManaged<{3}>(preciate);
            builder._transition._conditions.Add(condition);
            return builder;
        }}

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnExit<TS, TT, TR, {1}>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<{3}> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<{1}>
{2}
        {{
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<{3}>(callback);
            builder._state._onExit.Add(config);
            return builder;
        }}

        public static StateMachine<TS, TT>.Builder<TR>.StateBuilder ExecuteOnEnter<TS, TT, TR, {1}>(
                this StateMachine<TS, TT>.Builder<TR>.StateBuilder builder,
                CallbackManaged<{3}> callback)
            where TS : unmanaged
            where TT : unmanaged, StateMachine<TS, TT>.ITag
            where TR : class, StateMachine<TS, TT>.IIndexedRow, IEntityRow<{1}>
{2}
        {{
            var config = new StateMachine<TS, TT>.CallbackConfigManaged<{3}>(callback);
            builder._state._onEnter.Add(config);
            return builder;
        }}
";

        public string Generate(string template, bool native)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i <= 4; ++i)
            {
                for (int j = 1; j <= i; ++j)
                    builder.Append(Generate(template, i, j, native));
            }

            return builder.ToString();
        }

        public StringBuilder Generate(string template, int num, int target, bool native)
        {
            var genericTypeList = "T{0}".Repeat(", ", num);

            StringBuilder typeConstraintList = new StringBuilder();

            for (int i = 1; i <= num; ++i)
            {
                string format = "                where T{0} : struct, IEntityComponent";

                if (i == target && native)
                    format = "                where T{0} : unmanaged, IEntityComponent";
                else if (i == target && !native)
                    format = "                where T{0} : struct, IEntityViewComponent";

                typeConstraintList.AppendFormat(format, i);
            }

            var builder = new StringBuilder();
            builder.AppendFormat(template, "unused", genericTypeList, typeConstraintList, $"T{target}");
            return builder;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string source = $@" // Auto-generated code
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schema.Internal;

namespace Svelto.ECS.Schema
{{
    public static class StateMachineConfigNativeExtensions
    {{
{Generate(StateMachineConfigNativeTemplate, true)}
    }}

    public static class StateMachineConfigManagedExtensions
    {{
{Generate(StateMachineConfigManagedTemplate, false)}
    }}
}}";

            context.AddSource("StateMachine.Setting.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
