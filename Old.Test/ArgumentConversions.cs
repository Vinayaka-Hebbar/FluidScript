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
        internal static readonly Conversion[] Empty = new Conversion[0];
        private Conversion[] _items;
        private int size;
        private int start;

        public ArgumentConversions(int capacity)
        {
            if (capacity < 0)
                throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity == 0)
                _items = Empty;
            else
                _items = new Conversion[capacity];
        }

        public ArgumentConversions()
        {
            _items = Empty;
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
                throw new System.ArgumentOutOfRangeException(nameof(index));
            }
            if (size == _items.Length) EnsureCapacity(size + 1);
            if (index < size)
            {
                System.Array.Copy(_items, index, _items, index + 1, size - index);
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

        /// <summary>
        /// Revert back to last backup position <see cref="Backup"/>
        /// </summary>
        /// <returns></returns>
        public bool Recycle()
        {
            // Retail the array values ok
            size = start;
            return false;
        }

        public object[] Invoke(object[] values)
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
                                values = (object[])conversion.Invoke(values);
                                return values;
                        }
                    }
                }
            }
            return values;
        }

        public Conversion Find(int index)
        {
            for (int i = 0; i < size; i++)
            {
                Conversion item = _items[i];
                if (item.Index == index)
                    return item;
            }
            return null;
        }

        public ConversionGroups ToGroup()
        {
            var builder = new ConversionGroups();
            for (int i = 0; i < size; i++)
            {
                builder.Add(_items[i]);
            }
            return builder;
        }

        /// <summary>
        /// Save the position
        /// </summary>
        internal void Backup()
        {
            start = size;
        }

        public int Count { get => size; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public IEnumerator<Conversion> GetEnumerator()
        {
            return new Enumerator(this);
        }

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
                    return current;
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
