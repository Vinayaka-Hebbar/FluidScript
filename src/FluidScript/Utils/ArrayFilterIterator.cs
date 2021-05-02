using System;
using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Utils
{
    public struct ArrayFilterIterator<TSource, TFilter> : IEnumerable<TSource>, IEnumerator<TSource>
    {
        TSource[] source;
        Func<TSource, TFilter, bool> predicate;
        int index;
        readonly TFilter filter;
        private TSource current;

        public ArrayFilterIterator(TSource[] source, Func<TSource, TFilter, bool> predicate, TFilter filter)
        {
            this.source = source;
            this.filter = filter;
            this.predicate = predicate;
            current = default(TSource);
            index = 0;
        }

        public bool MoveNext()
        {
            while (index < source.Length)
            {
                TSource item = source[index];
                index++;
                if (predicate(item, filter))
                {
                    current = item;
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            current = default(TSource);
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return this;
        }

        object IEnumerator.Current
        {
            get { return current; }
        }

        public TSource Current => current;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IEnumerator.Reset()
        {
            index = 0;
        }

        public TSource[] ToArray()
        {
            TSource[] items = null;
            int count = 0;
            foreach (var item in source)
            {
                if (items == null)
                {
                    items = new TSource[4];
                }
                else if (items.Length == count)
                {
                    TSource[] newItems = new TSource[checked(count * 2)];
                    Array.Copy(items, 0, newItems, 0, count);
                    items = newItems;
                }
                if (predicate(item, filter))
                {
                    items[count] = item;
                    count++;
                }
            }
            if (count == 0) return new TSource[0];
            if (items.Length == count) return items;
            TSource[] result = new TSource[count];
            Array.Copy(items, 0, result, 0, count);
            return result;
        }
    }
}
