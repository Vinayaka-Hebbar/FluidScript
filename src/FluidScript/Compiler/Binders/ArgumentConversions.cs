using System;
using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Compiler.Binders
{
    /// <summary>
    /// Argument convert list
    /// </summary>
    public sealed class ArgumentConversions : IEnumerable<Conversion>
    {
        private const int _defaultCapacity = 4;
        internal const int MaxArrayLength = 0X7FEFFFFF;
        static readonly Conversion[] _emptyArray = new Conversion[0];
        private Conversion[] _items;
        private int size;

        public ArgumentConversions(int capacity)
        {
            if (capacity < 0)
                throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity == 0)
                _items = _emptyArray;
            else
                _items = new Conversion[capacity];
        }

        public ArgumentConversions()
        {
            _items = _emptyArray;
        }

        public void Add(Conversion item)
        {
            if (size == _items.Length) EnsureCapacity(size + 1);
            _items[size++] = item;
        }

        public void Insert(int index, Conversion item)
        {
            if ((uint)index > (uint)size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (size == _items.Length) EnsureCapacity(size + 1);
            if (index < size)
            {
                Array.Copy(_items, index, _items, index + 1, size - index);
            }
            _items[index] = item;
            size++;
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
                Conversion[] newItems = new Conversion[newCapacity];
                if (size > 0)
                    System.Array.Copy(_items, 0, newItems, 0, size);
                _items = newItems;
            }
        }

        public Conversion At(int index)
        {
            if (index < size)
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

        IEnumerator<Conversion> IEnumerable<Conversion>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public TList Invoke<TList>(TList values) where TList : IList
        {
            if (size > 0)
            {
                for (int i = 0; i < size; i++)
                {
                    var conversion = _items[i];
                    if (conversion != null)
                    {
                        int index = conversion.Index;
                        switch (conversion.ConversionType)
                        {
                            case ConversionType.Convert:
                                var arg = values[index];
                                values[index] = conversion.Invoke(arg);
                                break;
                            case ConversionType.ParamArray:
                                values[index] = conversion.Invoke(values);
                                return values;
                        }
                    }
                }
            }
            return values;
        }

        public int Count { get => size; }

        /// <summary>
        /// Enumerates the elements of a <see cref="ArgumentConversions"/>.
        /// </summary>
        internal struct Enumerator : IEnumerator<Conversion>
        {
            private readonly ArgumentConversions list;
            private int index;
            private Conversion current;

            internal Enumerator(ArgumentConversions list)
            {
                this.list = list;
                index = 0;
                current = null;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public Conversion Current
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
                ArgumentConversions localList = list;

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
