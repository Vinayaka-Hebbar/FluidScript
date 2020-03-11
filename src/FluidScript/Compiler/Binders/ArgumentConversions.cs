using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Compiler.Binders
{
    /// <summary>
    /// Argument convert list
    /// </summary>
    public sealed class ArgumenConversions : IEnumerable<ArgumentConversion>
    {
        private const int _defaultCapacity = 4;
        internal const int MaxArrayLength = 0X7FEFFFFF;
        static readonly ArgumentConversion[] _emptyArray = new ArgumentConversion[0];
        private ArgumentConversion[] _items;
        private int size;

        public ArgumenConversions(int capacity)
        {
            if (capacity < 0)
                throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity == 0)
                _items = _emptyArray;
            else
                _items = new ArgumentConversion[capacity];
        }

        public ArgumenConversions()
        {
            _items = _emptyArray;
        }

        public void Add(ArgumentConversion item)
        {
            if (size == _items.Length) EnsureCapacity(size + 1);
            _items[size++] = item;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
                // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
                if (newCapacity < min) newCapacity = min;
                ArgumentConversion[] newItems = new ArgumentConversion[newCapacity];
                if (size > 0)
                    System.Array.Copy(_items, 0, newItems, 0, size);
                _items = newItems;
            }
        }

        public ArgumentConversion At(int index)
        {
            if(index < size)
            {
                return _items[index];
            }
            return null;
        }

        public void Clear()
        {
            // Retail the array values ok
            size = 0;
        }

        IEnumerator<ArgumentConversion> IEnumerable<ArgumentConversion>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int Count { get => size; }

        /// <summary>
        /// Enumerates the elements of a <see cref="ArgumenConversions"/>.
        /// </summary>
        internal struct Enumerator : IEnumerator<ArgumentConversion>
        {
            private readonly ArgumenConversions list;
            private int index;
            private ArgumentConversion current;

            internal Enumerator(ArgumenConversions list)
            {
                this.list = list;
                index = 0;
                current = null;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public ArgumentConversion Current
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
                ArgumenConversions localList = list;

                if ((uint)index < (uint)localList.size)
                {
                    current = localList._items[index++];
                    // skip if null
                    if (current == null)
                        return MoveNext();
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

            void System.Collections.IEnumerator.Reset()
            {
                index = 0;
                current = null;
            }
        }
    }
}
