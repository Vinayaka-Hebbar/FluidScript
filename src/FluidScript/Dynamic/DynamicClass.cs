using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    [System.Serializable]
    internal sealed class DynamicClass : ICollection<LocalVariable>
    {
        private const int _defaultCapacity = 4;
        internal const int MaxArrayLength = 0X7FEFFFFF;
        static readonly LocalVariable[] _emptyArray = new LocalVariable[0];
        private LocalVariable[] _items;
        private int _size;

        [System.NonSerialized]
        internal readonly object Instance;

        [System.NonSerialized]
        internal readonly DynamicClass Parent;

        internal DynamicClass(object instance)
        {
            _items = _emptyArray;
            Instance = instance;
        }

        internal DynamicClass()
        {
            _items = _emptyArray;
        }

        internal DynamicClass(DynamicClass other)
        {
            Instance = other.Instance;
            _items = _emptyArray;
            Parent = other ?? throw new System.ArgumentNullException(nameof(other));
        }

        #region List

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        public LocalVariable this[int index]
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

        /// <summary>
        /// Read-only property describing how many elements are in the List.
        /// </summary>
        public int Count
        {
            get
            {
                return _size;
            }
        }

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold
        /// without resizing.
        /// </summary>
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
                        LocalVariable[] newItems = new LocalVariable[value];
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

        public bool IsReadOnly => false;

        #endregion
        /// <summary>
        /// Get Member of current context and parent context
        /// </summary>
        internal bool TryLookVariable(string name, out LocalVariable variable)
        {
            for (int index = 0; index < _size; index++)
            {
                var item = _items[index];
                if (item.Equals(name))
                {
                    variable = item;
                    return true;
                }
            }
            if (Parent != null)
                return Parent.TryLookVariable(name, out variable);
            variable = LocalVariable.Empty;
            return false;
        }

        internal IEnumerable<LocalVariable> LookVariables(string name, System.Func<LocalVariable, bool> predicat)
        {
            for (int index = 0; index < _size; index++)
            {
                var item = _items[index];
                if (item.Equals(name) && predicat(item))
                {
                    yield return item;
                }
            }
        }

        public LocalVariable Create(string name, System.Type type)
        {
            var local = new LocalVariable(name, Count, type);
            Add(local);
            return local;
        }

        public void Add(LocalVariable item)
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

        private void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index));
            }
            _size--;
            if (index < _size)
            {
                System.Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            _items[_size] = LocalVariable.Empty;
        }

        /// <summary>
        /// Inserts an element into the System.Collections.Generic.List`1 at the specified
        /// index.
        /// </summary>
        /// <param name="item">The zero-based index at which item should be inserted.</param>
        /// <returns>
        /// The object to insert. The value can be null for reference types.
        /// </returns>
        public int IndexOf(LocalVariable item)
        {
            return System.Array.IndexOf(_items, item, 0, _size);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="List{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="List{T}"/>. The value can
        ///     be null for reference types.
        /// </param>
        /// <returns>
        /// true if item is successfully removed; otherwise, false. This method also returns
        /// false if item was not found in the <see cref="List{T}"/>.
        /// </returns>
        public bool Remove(LocalVariable item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all elements from the <see cref="List{T}"/>
        /// </summary>
        public void Clear()
        {
            if (_size > 0)
            {
                // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                System.Array.Clear(_items, 0, _size);
                _size = 0;
            }
        }

        public bool Contains(LocalVariable obj)
        {
            return Exists(v => v.Equals(obj));
        }

        public bool Exists(System.Predicate<LocalVariable> match)
        {
            return FindIndex(0, _size, match) != -1;
        }

        private int FindIndex(int startIndex, int count, System.Predicate<LocalVariable> match)
        {
            if ((uint)startIndex > (uint)_size)
            {
                throw new System.ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (count < 0 || startIndex > _size - count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new System.ArgumentNullException(nameof(match));
            }

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(_items[i])) return i;
            }
            return -1;
        }

        void ICollection<LocalVariable>.CopyTo(LocalVariable[] array, int arrayIndex)
        {
            // Delegate rest of error checking to Array.Copy.
            System.Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        IEnumerator<LocalVariable> IEnumerable<LocalVariable>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="List{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator, IEnumerator<LocalVariable>
        {
            private readonly DynamicClass instance;
            private int index;
            private LocalVariable current;

            internal Enumerator(DynamicClass instance)
            {
                this.instance = instance;
                index = 0;
                current = LocalVariable.Empty;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            [Runtime.Register("current")]
            public LocalVariable Current
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
                    if (index == 0 || index == instance._size + 1)
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
                DynamicClass localList = instance;

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
                index = instance._size + 1;
                current = LocalVariable.Empty;
                return false;
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = LocalVariable.Empty;
            }
        }
    }
}
