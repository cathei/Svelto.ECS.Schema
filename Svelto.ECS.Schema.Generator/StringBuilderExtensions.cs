using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Svelto.ECS.Schema.Generator
{
    public static class StringBuilderExtensions
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
    }
}
