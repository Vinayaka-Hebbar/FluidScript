using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;

namespace FluidScript.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Iterate items
        /// </summary>
        public static void Iterate<TSource>(this IEnumerable<TSource> sources, System.Action<TSource> callback)
        {
            foreach (var item in sources)
            {
                callback(item);
            }
        }

        /// <summary>
        /// Iterate items
        /// </summary>
        public static void Iterate<TSource>(this IEnumerable<TSource> sources, System.Action<TSource, int> callback)
        {
            int index = 0;
            foreach (var item in sources)
            {
                callback(item, index++);
            }
        }

        /// <summary>
        /// Iterate items
        /// </summary>
        public static void ForEach<TSource>(this INodeList<TSource> sources, System.Action<TSource, int> callback) where TSource : Node
        {
            int size = sources.Count;
            for (int i = 0; i < size; i++)
            {
                callback(sources[i], i);
            }
        }
    }
}
