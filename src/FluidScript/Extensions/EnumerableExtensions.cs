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
    }
}
