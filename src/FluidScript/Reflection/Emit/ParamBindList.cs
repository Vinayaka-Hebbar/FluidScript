using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Reflection.Emit
{
    public sealed class ParamBindList : IEnumerable<ParamBind>
    {
        private const int _defaultCapacity = 4;
        internal const int MaxArrayLength = 0X7FEFFFFF;
        static readonly ParamBind[] _emptyArray = new ParamBind[0];
        private ParamBind[] _items;
        private int _size;

        public ParamBindList(int capacity)
        {
            if (capacity < 0)
                throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity == 0)
                _items = _emptyArray;
            else
                _items = new ParamBind[capacity];
        }

        public ParamBindList()
        {
            _items = _emptyArray;
        }

        public ParamBind this[int index]
        {
            get
            {
                // Following trick can reduce the range check by one
                if ((uint)index >= (uint)_size)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(index));
                }
                return _items[index];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(index));
                }
                _items[index] = value;
            }
        }

        public void Add(ParamBind item)
        {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
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
                Capacity = newCapacity;
            }
        }

        public void Clear()
        {
            if (_size > 0)
            {
                // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                System.Array.Clear(_items, 0, _size);
                _size = 0;
            }
        }

        IEnumerator<ParamBind> IEnumerable<ParamBind>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int Capacity
        {
            get
            {
                return _items.Length;
            }
            set
            {
                if (value < _size)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        ParamBind[] newItems = new ParamBind[value];
                        if (_size > 0)
                        {
                            System.Array.Copy(_items, 0, newItems, 0, _size);
                        }
                        _items = newItems;
                    }
                    else
                    {
                        _items = _emptyArray;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="List{T}"/>.
        /// </summary>
        internal struct Enumerator : IEnumerator<ParamBind>
        {
            private readonly ParamBindList list;
            private int index;
            private ParamBind current;

            internal Enumerator(ParamBindList list)
            {
                this.list = list;
                index = 0;
                current = null;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public ParamBind Current
            {
                get
                {
                    return current;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == list._size + 1)
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
                ParamBindList localList = list;

                if ((uint)index < (uint)localList._size)
                {
                    current = localList._items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list._size + 1;
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
