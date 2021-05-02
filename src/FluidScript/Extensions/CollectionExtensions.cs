using System.Collections.Generic;

namespace FluidScript.Extensions
{
    public static class CollectionExtensions
    {
        public static T[] AddFirst<T>(this IList<T> list, T item)
        {
            T[] res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
            return res;
        }

        public static T[] AddLast<T>(this IList<T> list, T item)
        {
            T[] res = new T[list.Count + 1];
            list.CopyTo(res, 0);
            res[list.Count] = item;
            return res;
        }

        public static U[] Map<T, U>(this ICollection<T> collection, System.Func<T, U> select)
        {
            int count = collection.Count;
            U[] result = new U[count];
            count = 0;
            foreach (T t in collection)
            {
                result[count++] = select(t);
            }
            return result;
        }
    }
}
