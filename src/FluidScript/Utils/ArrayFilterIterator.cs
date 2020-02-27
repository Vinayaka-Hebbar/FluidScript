using System;
using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Utils
{
    public class ArrayFilterIterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
    {
        TSource[] source;
        Func<TSource, object, bool> predicate;
        int index;
        readonly object filter;
        readonly int threadId;
        private int state;
        private TSource current;

        public ArrayFilterIterator(TSource[] source, Func<TSource, object, bool> predicate, object filter)
        {
            this.source = source;
            this.filter = filter;
            this.predicate = predicate;
            threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public ArrayFilterIterator<TSource> Clone()
        {
            return new ArrayFilterIterator<TSource>(source, predicate, filter);
        }

        public bool MoveNext()
        {
            if (state == 1)
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
                Dispose();
            }
            return false;
        }

        public void Dispose()
        {
            current = default(TSource);
            state = -1;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            if (threadId == System.Threading.Thread.CurrentThread.ManagedThreadId && state == 0)
            {
                state = 1;
                return this;
            }
            var duplicate = Clone();
            duplicate.state = 1;
            return duplicate;
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
            throw new NotImplementedException();
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
