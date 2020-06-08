using System.Runtime.CompilerServices;

namespace FluidScript.Collections
{
    /// <summary>
    /// Represents a strongly typed list of objects that can be accessed by index. Provides
    /// methods to search, sort, and manipulate lists.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [System.Serializable]
    [Runtime.Register(nameof(System.Array))]
    public class List<T> : FSObject, System.Collections.IList, System.Collections.Generic.ICollection<T>
        , System.Collections.IEnumerable, System.Collections.Generic.IEnumerable<T>
    {
        private const int _defaultCapacity = 4;
        public const int MaxArrayLength = 0X7FEFFFFF;
        static readonly T[] _emptyArray = new T[0];
        private T[] _items;

        private int _size;
        private int _version;

        private object _syncRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="List{T}"/> class that
        /// is empty and has the default initial capacity.
        /// </summary>
        public List()
        {
            _items = _emptyArray;
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="List{T}"/>
        /// </summary>
        /// <param name="capacity">
        /// The object to be added to the end of the System.Collections.Generic.List`1. The
        /// value can be null for reference types.
        /// </param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public List(Integer capacity)
        {
            var c = capacity.m_value;
            if (c < 0)
                throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (c == 0)
                _items = _emptyArray;
            else
                _items = new T[c];
        }

        /// <summary>
        /// Constructs a List, copying the contents of the given collection. The
        /// size and capacity of the new list will both be equal to the size of the
        /// given collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list</param>
        /// <exception cref="System.ArgumentNullException">capacity is less than 0.</exception>
        public List(System.Collections.Generic.IEnumerable<T> collection)
        {
            if (collection == null)
                throw new System.ArgumentNullException(nameof(collection));

            if (collection is System.Collections.Generic.ICollection<T> c)
            {
                int count = c.Count;
                if (count == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = new T[count];
                    c.CopyTo(_items, 0);
                    _size = count;
                }
            }
            else
            {
                _size = 0;
                _items = _emptyArray;
                // This enumerable could be empty.  Let Add allocate a new array, if needed.
                // Note it will also go to _defaultCapacity first, not 1, then 2, etc.

                using (var en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        }

        /// <summary>
        /// Read-only property describing how many elements are in the List.
        /// </summary>
        [Runtime.Register("count")]
        public int Count
        {
            get
            {
                return _size;
            }
        }

        bool System.Collections.IList.IsFixedSize
        {
            get { return false; }
        }

        bool System.Collections.IList.IsReadOnly
        {
            get { return false; }
        }

        // Is this List synchronized (thread-safe)?
        bool System.Collections.ICollection.IsSynchronized
        {
            get { return false; }
        }

        // Synchronization root for this object.
        object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        bool System.Collections.Generic.ICollection<T>.IsReadOnly => false;

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
                // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
                if (newCapacity < min) newCapacity = min;
                T[] newItems = new T[newCapacity];
                if (_size > 0)
                    System.Array.Copy(_items, 0, newItems, 0, _size);
                _items = newItems;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        public T this[Integer index]
        {
            get
            {
                int i = index.m_value;
                // Following trick can reduce the range check by one
                if ((uint)i >= (uint)_size)
                {
                    // handle execption by returning default
                    return default(T);
                }
                return _items[i];
            }
            set
            {
                var i = index.m_value;
                // if capacity is more it will fit
                if ((uint)i >= (uint)_size)
                {
                    // is array has enough capacity
                    if (_items.Length <= i)
                        EnsureCapacity(i + 1);
                    _size = i;
                    _size++;
                }
                _items[i] = value;
                _version++;
                //fault
            }
        }

        object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (value == null && !(default(T) == null))
                    throw new System.ArgumentNullException(nameof(value));

                try
                {
                    this[index] = (T)value;
                }
                catch (System.InvalidCastException)
                {
                    throw new System.ArrayTypeMismatchException(nameof(T));
                }
            }
        }


        /// <summary>
        /// Adds the given object to the end of this list. The size of the list is
        /// increased by one. If required, the capacity of the list is doubled
        /// before adding the new element.
        /// </summary>
        [Runtime.Register("add")]
        public void Add(T item)
        {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
            _version++;
        }

        int System.Collections.IList.Add(object item)
        {
            if (item == null && !(default(T) == null))
                throw new System.ArgumentNullException(nameof(item));

            try
            {
                Add((T)item);
            }
            catch (System.InvalidCastException)
            {
                throw new System.ArrayTypeMismatchException(nameof(T));
            }
            return Count - 1;
        }

        /// <summary>
        /// Adds the elements of the given collection to the end of this list. If
        /// required, the capacity of the list is increased to twice the previous
        /// capacity or the new size, whichever is larger.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the System.Collections.Generic.List`1.
        /// The collection itself cannot be null, but it can contain elements that are null,
        /// if type T is a reference type
        /// </param>
        /// <exception cref="System.ArgumentNullException">collection is null.</exception>
        [Runtime.Register("addRange")]
        public void AddRange(System.Collections.Generic.IEnumerable<T> collection)
        {
            if (collection == null)
                throw new System.ArgumentNullException(nameof(collection));

            if (collection is System.Collections.Generic.ICollection<T> c)
            {
                int count = c.Count;
                if (count > 0)
                {
                    var index = _size;
                    EnsureCapacity(count + index);
                    c.CopyTo(_items, index);
                    _size += count;
                }
            }
            else
            {
                // This enumerable could be empty.  Let Add allocate a new array, if needed.
                // Note it will also go to _defaultCapacity first, not 1, then 2, etc.

                using (System.Collections.Generic.IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        }

        // Inserts an element into this list at a given index. The size of the list
        // is increased by one. If required, the capacity of the list is doubled
        // before inserting the new element.
        // 
        /// <summary>
        /// Inserts an element into the System.Collections.Generic.List`1 at the specified
        /// index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        [Runtime.Register("insert")]
        public void Insert(Integer index, T item)
        {
            int i = index.m_value;
            // Note that insertions at the end are legal.
            if ((uint)i > (uint)_size)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index));
            }
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            if (i < _size)
            {
                System.Array.Copy(_items, i, _items, i + 1, _size - i);
            }
            _items[i] = item;
            _size++;
            _version++;
        }

        void System.Collections.IList.Insert(int index, object item)
        {
            if (item == null && !(default(T) == null))
                throw new System.ArgumentNullException(nameof(item));

            try
            {
                Insert(index, (T)item);
            }
            catch (System.InvalidCastException)
            {
                throw new System.ArrayTypeMismatchException(nameof(T));
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="List{T}"/>
        /// </summary>
        [Runtime.Register("clear")]
        public void Clear()
        {
            if (_size > 0)
            {
                // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                System.Array.Clear(_items, 0, _size);
                _size = 0;
            }
            _version++;
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="List{T}"/>
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="List{T}"/>. The value can
        /// be null for reference types.
        /// </param>
        /// <returns>
        /// true if item is found in the <see cref="List{T}"/>; otherwise, false.
        /// </returns>
        [Runtime.Register("contains")]
        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < _size; i++)
                    if (_items[i] == null)
                        return true;
                return false;
            }
            else
            {
                System.Collections.Generic.EqualityComparer<T> c = System.Collections.Generic.EqualityComparer<T>.Default;
                for (int i = 0; i < _size; i++)
                {
                    if (c.Equals(_items[i], item)) return true;
                }
                return false;
            }
        }

        bool System.Collections.IList.Contains(object item)
        {
            if (IsCompatibleObject(item))
            {
                return Contains((T)item);
            }
            return false;
        }

        /// <summary>
        /// Copies the entire <see cref="List{T}"/> to a compatible one-dimensional
        /// array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional System.Array that is the destination of the elements copied
        /// from <see cref="List{T}"/>. The System.Array must have zero-based
        /// indexing.
        /// </param>
        [Runtime.Register("copyTo")]
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the entire <see cref="List{T}"/> to a compatible one-dimensional
        /// array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional System.Array that is the destination of the elements copied
        /// from <see cref="List{T}"/>. The System.Array must have zero-based
        /// indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        [Runtime.Register("copyTo")]
        public void CopyTo(T[] array, int arrayIndex)
        {
            // Delegate rest of error checking to Array.Copy.
            System.Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        void System.Collections.ICollection.CopyTo(System.Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new System.ArgumentException(nameof(array.Rank));
            }

            try
            {
                // Array.Copy will check for NULL.
                System.Array.Copy(_items, 0, array, arrayIndex, _size);
            }
            catch (System.ArrayTypeMismatchException)
            {
                throw new System.ArgumentException(nameof(array));
            }
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
        [Runtime.Register("remove")]
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        void System.Collections.IList.Remove(object item)
        {
            if (IsCompatibleObject(item))
            {
                Remove((T)item);
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="List{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        [Runtime.Register("removeAt")]
        public void RemoveAt(int index)
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
            _items[_size] = default(T);
            _version++;
        }

        /// <summary>
        /// Inserts an element into the System.Collections.Generic.List`1 at the specified
        /// index.
        /// </summary>
        /// <param name="item">The zero-based index at which item should be inserted.</param>
        /// <returns>
        /// The object to insert. The value can be null for reference types.
        /// </returns>
        [Runtime.Register("indexOf")]
        public int IndexOf(T item)
        {
            return System.Array.IndexOf(_items, item, 0, _size);
        }

        int System.Collections.IList.IndexOf(object item)
        {
            if (IsCompatibleObject(item))
            {
                return IndexOf((T)item);
            }
            return -1;
        }

        public T[] ToArray()
        {
            T[] array = new T[_size];
            System.Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        [Runtime.Register("clone")]
        public List<T> Clone()
        {
            List<T> clone = new List<T>(_size);
            for (int i = 0; i < _size; i++)
            {
                clone.Add(_items[i]);
            }
            return clone;
        }

        public override string ToString()
        {
            return string.Join(",", System.Linq.Enumerable.Select(this, (item) => item?.ToString()));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="List{T}"/>.
        /// </summary>
        /// <returns>
        ///  A <see cref="List{T}.Iterator"/> for the <see cref="List{T}"/>.
        /// </returns>
        [Runtime.Register("iterator")]
        public Iterator GetEnumerator()
        {
            return new Iterator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Iterator(this);
        }

        System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        {
            return new Iterator(this);
        }

        #region Runtime
        [Runtime.Register("forEach")]
        public void ForEach(System.Delegate iterate)
        {
            for (int i = 0; i < _size; i++)
            {
                iterate.DynamicInvoke(_items[i]);
            }
        }
        #endregion

        /// <summary>
        /// Enumerates the elements of a <see cref="List{T}"/>.
        /// </summary>
        [Runtime.Register(nameof(Iterator))]
        public struct Iterator : IFSObject, System.Collections.IEnumerator, System.Collections.Generic.IEnumerator<T>
        {
            private readonly List<T> list;
            private int index;
            private readonly int version;
            private T current;

            internal Iterator(List<T> list)
            {
                this.list = list;
                index = 0;
                version = list._version;
                current = default(T);
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            [Runtime.Register("current")]
            public T Current
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
            [Runtime.Register("moveNext")]
            public bool MoveNext()
            {
                List<T> localList = list;

                if (version == localList._version && ((uint)index < (uint)localList._size))
                {
                    current = localList._items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (version != list._version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                index = list._size + 1;
                current = default(T);
                return false;
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            [Runtime.Register("hashCode")]
            Integer IFSObject.GetHashCode()
            {
                return GetHashCode();
            }

            /// <summary>
            /// Returns the fully qualified type name of this instance.
            /// </summary>
            /// <returns>
            /// A System.String containing a fully qualified type name.
            /// </returns>
            [Runtime.Register("toString")]
            String IFSObject.ToString()
            {
                return ToString();
            }

            /// <summary>
            /// Determines whether the specified System.Object is equal to the current System.Object.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns>
            /// true if the specified System.Object is equal to the current System.Object; otherwise,
            /// false.
            /// </returns>
            [Runtime.Register("equals")]
            Boolean IFSObject.Equals(object obj)
            {
                return ReferenceEquals(this, obj);
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (version != list._version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                index = 0;
                current = default(T);
            }
        }
    }
}
