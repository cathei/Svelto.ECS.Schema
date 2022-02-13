using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    public static class SourceGeneratorExtensions
    {
        // public static StringBuilder AppendTab(this StringBuilder builder, int tab)
        // {
        //     for (int i = 0; i < tab; ++i)
        //         builder.Append("    ");

        //     return builder;
        // }

        public static StringBuilder Repeat(this string format, int num)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i <= num; ++i)
            {
                builder.AppendFormat(format, i);
            }

            return builder;
        }

        public static StringBuilder Join(this string format, string joiner, int num)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i <= num; ++i)
            {
                builder.AppendFormat(format, i);
                if (i < num)
                    builder.Append(joiner);
            }

            return builder;
        }


        private const string RootPath = "../Svelto.ECS.Schema";

        public static void SaveToFile(this GeneratorExecutionContext context, string filePath, string source)
        {
            // I know it is modern this way...
            // context.AddSource("IndexExtensions.g.cs", source);

            // However I prefer just save it as file because
            // 0. I do not use runtime code analysis
            // 1. Intelisence will work without any problem
            // 2. Source is directly visible so I can see problem if generation step is wrong
            // is there a way to achieve this without build multiple times?
            File.WriteAllText(Path.Combine(RootPath, filePath), source);
        }
    }
}
