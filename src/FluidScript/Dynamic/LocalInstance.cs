using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace FluidScript.Dynamic
{
    [System.Serializable]
    internal sealed class LocalInstance : IDictionary<string, object>, System.Collections.IDictionary, IDynamicMetaObjectProvider, ISerializable
    {
        private const int _defaultCapacity = 4;
        internal const int MaxArrayLength = 0X7FEFFFFF;
        static readonly LocalVariable[] _emptyArray = new LocalVariable[0];
        private LocalVariable[] _items;
        private int _size;
        private int _version;
        /// <summary>
        /// Current Block
        /// </summary>
        [System.NonSerialized]
        internal LocalContext Current;

        [System.NonSerialized]
        internal readonly object Instance;

        internal LocalInstance(object instance)
        {
            _items = _emptyArray;
            Instance = instance;
            Current = new LocalContext(this);
        }

        internal LocalInstance(LocalInstance other)
        {
            Instance = other.Instance;
            if (other == null)
                throw new System.ArgumentNullException(nameof(other));

            var c = other._items;
            int count = other._size;
            if (count == 0)
            {
                _items = _emptyArray;
            }
            else
            {
                _items = new LocalVariable[count];
                System.Array.Copy(c, 0, _items, 0, count);
                _size = count;
            }
            Current = other.Current;
        }

        public IEnumerable<LocalVariable> Variables
        {
            get
            {
                for (int index = 0; index < _size; index++)
                {
                    yield return _items[index];
                }
            }
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
                _version++;
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

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold
        /// without resizing.
        /// </summary>
        [Runtime.Register("capacity")]
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

        public ICollection<string> Keys
        {
            get
            {
                var variables = Current.Keys;
                string[] keys = new string[variables.Count];
                int index = 0;
                foreach (var variable in variables)
                {
                    keys[index++] = variable.Name;
                }
                return keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                var current = Current;
                var variables = current.Keys;
                object[] items = new object[variables.Count];
                var index = 0;
                foreach (var variable in variables)
                {
                    items[index++] = current.GetValue(variable);

                }
                return items;
            }
        }

        public bool IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        ICollection IDictionary.Keys => (ICollection)Keys;

        ICollection IDictionary.Values => (ICollection)Values;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => null;

        object IDictionary.this[object key]
        {
            get
            {
                var name = key.ToString();
                return Current.Find(name);
            }

            set
            {
                var name = key.ToString();
                CreateOrModify(name, value);
            }
        }
        #endregion

        /// <summary>
        /// Getter and Setter of variables
        /// </summary>
        public object this[string name]
        {
            get => Current.Find(name);
            set => CreateOrModify(name, value);
        }

        internal object GetValue(LocalVariable variable)
        {
            return Current.GetValue(variable);
        }

        internal bool Contains(string name)
        {
            return Exists(v => v.Equals(name));
        }

        internal bool Contains(object obj)
        {
            return Exists(v => v.Equals(obj));
        }

        internal bool TryGetMember(string name, out LocalVariable variable)
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
            variable = LocalVariable.Empty;
            return false;
        }

        public LocalVariable Create(string name, System.Type type)
        {
            var local = new LocalVariable(name, Count, type);
            Add(local);
            return local;
        }

        public void Create(string name, System.Type type, object value)
        {
            var local = new LocalVariable(name, Count, type);
            Add(local);
            Current[local] = value;
        }

        /// <summary>
        /// Adds the given object to the end of this list. The size of the list is
        /// increased by one. If required, the capacity of the list is doubled
        /// before adding the new element.
        /// </summary>
        [Runtime.Register("add")]
        public void Add(LocalVariable item)
        {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
            _version++;
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

        internal void CreateOrModify(string name, object value)
        {
            LocalVariable? variable = null;
            for (int index = 0; index < _size; index++)
            {
                var item = _items[index];
                if (item.Equals(name))
                {
                    variable = item;
                    break;
                }
            }
            if (variable.HasValue == false)
            {
                // value not created
                variable = Create(name, value == null ? Reflection.TypeUtils.ObjectType : value.GetType());
                Current[variable.Value] = value;
                return;
            }
            Current.Modify(variable.Value, value);
        }

        internal LocalContext CreateContext()
        {
            return new LocalContext(this, Current);
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
            _items[_size] = LocalVariable.Empty;
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
        [Runtime.Register("remove")]
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

        internal bool Remove(string name)
        {
            int index = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Equals(name))
                {
                    index = i;
                    break;
                }
            }
            if (index > -1)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public LocalVariable[] ToArray()
        {
            LocalVariable[] array = new LocalVariable[_size];
            System.Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        public override string ToString()
        {
            return string.Join(",", System.Linq.Enumerable.Select(Current.Keys, (item)=> item.Name));
        }

        #region IDictionary
        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return Contains(key);
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            CreateOrModify(key, value);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            if (TryGetMember(key, out LocalVariable variable))
            {
                value = Current.GetValue(variable);
                return true;
            }
            value = null;
            return false;
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            CreateOrModify(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            var current = Current;
            foreach (LocalVariable item in current.Keys)
            {
                if (current.TryFind(item, out object value))
                {
                    array[arrayIndex++] = new KeyValuePair<string, object>(item.Name, value);
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key);
        }

        private static IEnumerator<KeyValuePair<string, object>> GetEnumerator(LocalInstance scope)
        {
            var current = scope.Current;
            foreach (LocalVariable item in current.Keys)
            {
                if (current.TryFind(item, out object value))
                {
                    yield return new KeyValuePair<string, object>(item.Name, value);
                }
            }
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(this);
        }

        void IDictionary.Add(object key, object value)
        {
            CreateOrModify(key.ToString(), value);
        }

        void IDictionary.Clear()
        {
            Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return Current.GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            int index = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Equals(key))
                {
                    index = i;
                    break;
                }
            }
            if (index > -1)
            {
                RemoveAt(index);
            }
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            System.Array.Copy(_items, 0, array, 0, _size);
        }

        #endregion

        #region Serializable
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //todo list serialize
            var current = Current;
            foreach (var item in current.Keys)
            {
                var value = current.GetValue(item);
                if (item.Type.IsPrimitive)
                    info.AddValue(item.Name, current.GetValue(item), item.Type);
                else if (value is System.IConvertible convertible)
                {
                    info.AddValue(item.Name, System.Convert.ChangeType(value, convertible.GetTypeCode()), item.Type);
                }
                else if (value is String)
                {
                    info.AddValue(item.Name, value.ToString(), typeof(string));
                }
                else
                {
                    info.AddValue(item.Name, value);
                }
            }
        }
        #endregion

        #region DynamicMetaObjectProvider
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new MetaObject(parameter, this);
        }
        #endregion
    }
}
