using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class NodeList<T> : INodeList<T> where T : Node
    {
        static readonly T[] EmptyNodeList = new T[0];
        int size;
        T[] items;

        public NodeList()
        {
            items = EmptyNodeList;
        }

        public NodeList(T[] items)
        {
            this.items = items;
            size = items.Length;
        }

        public T this[int index]
        {
            get
            {
                // Following trick can reduce the range check by one
                if ((uint)index >= (uint)size)
                    throw new System.ArgumentOutOfRangeException(nameof(index));
                return items[index];
            }
        }

        public int Length => size;

        public void Add(T expression)
        {
            if (size == items.Length)
            {
                int newCapacity = items.Length == 0 ? 4 : items.Length * 2;
                // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if (newCapacity > int.MaxValue) newCapacity = int.MaxValue;
                T[] newItems = new T[newCapacity];
                // srcPtr and destPtr are IntPtr's pointing to valid memory locations
                // size is the number of long (normally 4 bytes) to copy
                if (size > 0)
                    System.Array.Copy(items, 0, newItems, 0, size);
                items = newItems;
            }
            items[size++] = expression;
        }

        public TElement[] Map<TElement>(System.Func<T, TElement> predicate)
        {
            var res = new TElement[size];
            for (int i = 0; i < size; i++)
            {
                res[i] = predicate(items[i]);
            }
            return res;
        }

        public void ForEach(System.Action<T> selector)
        {
            for (int i = 0; i < size; i++)
            {
                selector(items[i]);
            }
        }

        public T[] ToArray()
        {
            var res = new T[size];
            System.Array.Copy(items, 0, res, 0, size);
            return res;
        }

        /// <summary>
        /// Copy to array starts from <paramref name="index"/>
        /// </summary>
        public void CopyTo(System.Array array, int index)
        {
            System.Array.Copy(items, index, array, 0, array.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        
        /// <summary>
        /// Enumerates the elements of a <see cref="NodeList{T}"/>.
        /// </summary>
        internal struct Enumerator : IEnumerator<T>
        {
            private readonly NodeList<T> list;
            private int index;
            private T current;

            internal Enumerator(NodeList<T> list)
            {
                this.list = list;
                index = 0;
                current = null;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public T Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == list.size + 1)
                    {
                        throw new System.InvalidOperationException(nameof(index));
                    }
                    return Current;
                }
            }

            /// <summary>
            /// Releases all resources used by the <see cref="List{T}"/>.Enumerator.
            /// </summary>
            public void Dispose()
            {

            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="List{T}"/>.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if
            /// the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                NodeList<T> localList = list;

                if ((uint)index < (uint)localList.size)
                {
                    current = localList.items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.size + 1;
                current = null;
                return false;
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = null;
            }
        }
    }
}
