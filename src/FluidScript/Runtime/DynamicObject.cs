using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace FluidScript.Runtime
{
    // todo create Object.Keys
    [System.Serializable]
    public class DynamicObject : Collections.DictionaryBase<LocalVariable, object>, IDictionary<string, object>, ISerializable, IDynamicMetaObjectProvider, IMetaObjectProvider
    {
        static readonly IEqualityComparer<LocalVariable> DefaultComparer = EqualityComparer<LocalVariable>.Default;

        public DynamicObject(int capacity) : base(capacity, DefaultComparer)
        {

        }

        public DynamicObject() : base(3, DefaultComparer)
        {
        }

        public DynamicObject(IDictionary<string, object> values) : base(3, DefaultComparer)
        {
            if (values == null)
            {
                throw new System.ArgumentNullException(nameof(values));
            }
            foreach (var item in values)
            {
                Add(item.Key, item.Value);
            }
        }

        #region List And Dictionary
        public ICollection<string> Keys
        {
            get
            {
                var size = Count;
                string[] keys = new string[size];
                for (int index = 0; index < size; index++)
                {
                    keys[index] = entries[index].Key.Name;
                }
                return keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                var size = Count;
                object[] values = new object[size];
                for (int index = 0; index < size; index++)
                {
                    values[index] = entries[index].Value;
                }
                return values;
            }
        }

        /// <summary>
        /// Getter and Setter of variables
        /// </summary>
        public object this[string name]
        {
            get
            {
                var i = FindEntry(name);
                if (i >= 0)
                {
                    return entries[i].Value;
                }
                return null;
            }
            set
            {
                var i = FindEntry(name);
                if (i >= 0)
                {
                    entries[i].Value = value;
                }
                else
                {
                    Add(name, value);
                }
            }
        }

        private int FindEntry(string key)
        {
            if (key == null)
            {
                throw new System.NullReferenceException(nameof(key));
            }
            if (buckets != null)
            {
                Entry[] items = entries;
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = items[i].Next)
                {
                    if (items[i].HashCode == hashCode && Equals(items[i].Key.Name, key)) return i;
                }
            }
            return -1;
        }

        [Register("contains")]
        public bool ContainsKey(string key)
        {
            return FindEntry(key) >= 0;
        }

        /// <summary>
        /// Get current context variable
        /// </summary>
        public bool TryGetMember(string key, out LocalVariable value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = entries[i].Key;
                return true;
            }
            value = LocalVariable.Empty;
            return false;
        }

        /// <summary>
        /// Replaces specific value if key exist
        /// </summary>
        public void Add(string key, object value)
        {
            var type = value == null ? Compiler.TypeProvider.ObjectType : value.GetType();
            Add(key, type, value);
        }

        internal LocalVariable Add(string key, System.Type type, object value)
        {
            if (key == null)
                throw new System.ArgumentNullException(nameof(key));
            return Insert(key, type, value);
        }

        internal void Update(LocalVariable local, object value)
        {
            int hashCode = local.GetHashCode();
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Equals(entries[i].Key.Name, local.Name))
                {
                    entries[i].Value = value;
                    version++;
                    return;
                }
            }
            throw new KeyNotFoundException(local.Name);
        }

        internal LocalVariable Insert(string name, System.Type type, object value)
        {
            if (buckets == null) Initialize(0);
            int hashCode = name.GetHashCode() & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Equals(entries[i].Key.Name, name))
                {
                    throw new System.ArgumentException(string.Format("shadow variable name '{0}'", name));
                }
            }
            int index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].Next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize(Helpers.ExpandPrime(count), false);
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;
            }
            var variable = new LocalVariable(name, type, index, hashCode);
            entries[index].HashCode = hashCode;
            entries[index].Next = buckets[targetBucket];
            entries[index].Key = variable;
            entries[index].Value = value;
            buckets[targetBucket] = index;
            return variable;
        }

        #endregion

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        #region Runtime Support

        [Register("keys")]
        public Collections.List<String> GetMemberNames()
        {
            var size = Count;
            var keys = new Collections.List<String>(size);
            for (int index = 0; index < size; index++)
            {
                var local = entries[index].Key;
                keys.Add(local.Name);
            }
            return keys;
        }

        [Register("clone")]
        public DynamicObject Clone()
        {
            return new DynamicObject(this);
        }

        [Register("toString")]
        public override string ToString()
        {
            return string.Join(",\n", Keys);
        }
        #endregion

        #region DictionaryEnumerator

        [System.Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IDictionaryEnumerator
        {
            private readonly DynamicObject obj;
            private readonly int version;
            private int index;
            private KeyValuePair<string, object> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(DynamicObject value, int getEnumeratorRetType)
            {
                obj = value;
                version = value.version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<string, object>();
            }

            public bool MoveNext()
            {
                if (version != obj.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)obj.count)
                {
                    if (obj.entries[index].HashCode >= 0)
                    {
                        current = new KeyValuePair<string, object>(obj.entries[index].Key.Name, obj.entries[index].Value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = obj.count + 1;
                current = new KeyValuePair<string, object>();
                return false;
            }

            public KeyValuePair<string, object> Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<string, object>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != obj.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                index = 0;
                current = new KeyValuePair<string, object>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return current.Value;
                }
            }
        }
        #endregion

        #region Serializable
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var size = Count;
            //todo list serialize
            for (int index = 0; index < size; index++)
            {
                var local = entries[index];
                if (local.HashCode >= 0)
                {
                    var key = local.Key;
                    var value = local.Value;
                    if (key.Type.IsPrimitive)
                        info.AddValue(key.Name, value, key.Type);
                    else if (value is System.IConvertible convertible)
                    {
                        info.AddValue(key.Name, System.Convert.ChangeType(value, convertible.GetTypeCode()), key.Type);
                    }
                    else if (value is String)
                    {
                        info.AddValue(key.Name, value.ToString(), typeof(string));
                    }
                    else
                    {
                        info.AddValue(key.Name, value);
                    }
                }
            }

        }
        #endregion

        #region DynamicMetaObjectProvider
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new MetaObject(parameter, this);
        }

        private MetaObjectProvider _runtime;
        MetaObjectProvider IMetaObjectProvider.GetMetaObject()
        {
            if (_runtime == null)
                _runtime = new MetaObjectProvider(this);
            return _runtime;
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = entries[i].Value;
                return true;
            }
            value = null;
            return false;
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            int i = FindEntry(item.Key);
            if (i >= 0)
            {
                var value = entries[i].Value;
                if (EqualityComparer<object>.Default.Equals(value, item.Value))
                    return true;
            }
            return false;
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int index)
        {
            if (array == null)
            {
                throw new System.ArgumentNullException(nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new System.ArgumentOutOfRangeException(string.Concat("Index ", index, " out of range"));
            }

            if (array.Length - index < Count)
            {
                throw new System.ArgumentException("array too small");
            }

            int count = Count;
            Entry[] entries = this.entries;
            for (int i = 0; i < count; i++)
            {
                var entry = entries[i];
                array[index++] = new KeyValuePair<string, object>(entry.Key.Name, entry.Value);
            }
        }

        public bool Remove(string key)
        {
            var i = FindEntry(key);
            if (i >= 0)
            {
                return Remove(entries[i].Key);
            }
            return false;
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            int i = FindEntry(item.Key);
            if (i >= 0)
            {
                var entry = entries[i];
                if (EqualityComparer<object>.Default.Equals(entry.Value, item.Value))
                    return Remove(entry.Key);
            }
            return false;
        }
        #endregion
    }
}