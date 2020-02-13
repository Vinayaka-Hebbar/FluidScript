using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace FluidScript.Dynamic
{
    // todo create Object.Keys
    [System.Serializable]
    public sealed class DynamicObject : IDictionary<string, object>, ISerializable, IDynamicMetaObjectProvider, IRuntimeMetaObjectProvider
    {
        private LocalVariable[] locals;

        private int size;
        private int version;
        private int[] buckets;

        internal readonly DynamicData Data;

        private DynamicObject(int capacity)
        {
            var size = capacity == 0 ? Utils.DictionaryHelpers.InitPrime : Utils.DictionaryHelpers.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            locals = new LocalVariable[size];
        }

        internal DynamicObject(DynamicData data) : this(0)
        {
            Data = data;
        }

        public DynamicObject() : this(0)
        {
            Data = new DynamicData(0);
        }

        #region List And Dictionary
        public ICollection<string> Keys
        {
            get
            {
                string[] keys = new string[size];
                for (int index = 0; index < size; index++)
                {
                    keys[index] = locals[index].Name;
                }
                return keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                object[] values = new object[size];
                for (int index = 0; index < size; index++)
                {
                    var entry = locals[index];
                    values[index] = Data[entry.Index];
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
                    return Data[locals[i].Index];
                }
                return null;
            }
            set
            {
                Insert(name, value == null ? Utils.TypeUtils.ObjectType : value.GetType(), value, false);
            }
        }

        private void Resize(int newSize)
        {
            //Contract.Assert(newSize >= entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            LocalVariable[] newLocals = new LocalVariable[newSize];
            System.Array.Copy(locals, 0, newLocals, 0, size);
            for (int i = 0; i < size; i++)
            {
                if (newLocals[i].HashToken >= 0)
                {
                    int bucket = newLocals[i].HashToken % newSize;
                    newLocals[i].Next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            locals = newLocals;
        }

        [Runtime.Register("count")]
        public int Count
        {
            get { return size; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;


        private int FindEntry(string key)
        {
            if (key == null)
            {
                throw new KeyNotFoundException(nameof(key));
            }
            if (buckets != null)
            {
                LocalVariable[] items = locals;
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = items[i].Next)
                {
                    if (items[i].HashToken == hashCode && Equals(items[i].Name, key)) return i;
                }
            }
            return -1;
        }

        [Runtime.Register("contains")]
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
                value = locals[i];
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
            Insert(key, value == null ? Utils.TypeUtils.ObjectType : value.GetType(), value, true);
        }

        internal void Update(LocalVariable local, object value)
        {
            int hashCode = local.HashToken;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = locals[i].Next)
            {
                if (locals[i].HashToken == hashCode && Equals(locals[i].Name, local.Name))
                {
                    var existed = locals[i];
                    Data[existed.Index] = value;
                    version++;
                    return;
                }
            }
            throw new KeyNotFoundException(local.Name);
        }

        internal void Insert(string key, System.Type type, object value, bool add)
        {
            int hashCode = key.GetHashCode() & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = locals[i].Next)
            {
                if (locals[i].HashToken == hashCode && Equals(locals[i].Name, key))
                {
                    if (add)
                        throw new System.ArgumentException(string.Format("shadow variable name '{0}'", key));
                    var existed = locals[i];
                    Data[existed.Index] = value;
                    version++;
                    return;
                }
            }
            var local = Data.Create(key, type, value);
            int index;
            if (size == locals.Length)
            {
                Resize(Utils.DictionaryHelpers.ExpandPrime(size));
                targetBucket = hashCode % buckets.Length;
            }
            index = size;
            size++;
            local.Next = buckets[targetBucket];
            locals[index] = local;
            buckets[targetBucket] = index;
            version++;
        }

        [Runtime.Register("clear")]
        public void Clear()
        {
            if (size > 0)
            {
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                // detach from Data
                int index = size - 1;
                LocalVariable[] locals = this.locals;
                for (; index > -1; index--)
                {
                    var entry = locals[index];
                    Data.Remove(entry);
                }
                // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                System.Array.Clear(locals, 0, size);
                size = 0;
            }
            version++;
        }

        internal void Detach()
        {
            if (size > 0)
            {
                // detach from Data
                int index = size - 1;
                LocalVariable[] locals = this.locals;
                for (; index > -1; index--)
                {
                    var entry = locals[index];
                    //todo removeAt
                    Data.Remove(entry);
                }
                // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                this.locals = null;
                buckets = null;
                size = 0;
            }
            version++;
        }

        #endregion

        public override string ToString()
        {
            return string.Join(",\n", Keys);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        #region Runtime Support

        [Runtime.Register("keys")]
        public Collections.List<String> GetMemberNames()
        {
            var keys = new Collections.List<String>(size);
            for (int index = 0; index < size; index++)
            {
                var local = locals[index];
                keys.Add(local.Name);
            }
            return keys;
        }
        #endregion

        #region DictionaryEnumerator

        [System.Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IDictionaryEnumerator
        {
            private readonly DynamicObject obj;
            private readonly DynamicData data;
            private readonly int version;
            private int index;
            private KeyValuePair<string, object> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(DynamicObject value, int getEnumeratorRetType)
            {
                obj = value;
                data = value.Data;
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
                while ((uint)index < (uint)obj.size)
                {
                    var local = obj.locals[index];
                    current = new KeyValuePair<string, object>(local.Name, data[local.Index]);
                    index++;
                    return true;
                }

                index = obj.size + 1;
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
                    if (index == 0 || (index == obj.size + 1))
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
                    if (index == 0 || (index == obj.size + 1))
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
                    if (index == 0 || (index == obj.size + 1))
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
                    if (index == 0 || (index == obj.size + 1))
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
            DynamicData data = this.Data;
            //todo list serialize
            for (int index = 0; index < size; index++)
            {
                var local = locals[index];
                var value = data[local.Index];
                if (local.Type.IsPrimitive)
                    info.AddValue(local.Name, value, local.Type);
                else if (value is System.IConvertible convertible)
                {
                    info.AddValue(local.Name, System.Convert.ChangeType(value, convertible.GetTypeCode()), local.Type);
                }
                else if (value is String)
                {
                    info.AddValue(local.Name, value.ToString(), typeof(string));
                }
                else
                {
                    info.AddValue(local.Name, value);
                }
            }

        }
        #endregion

        #region DynamicMetaObjectProvider
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new MetaObject(parameter, this);
        }

        private RuntimeMetaObject _runtime;
        RuntimeMetaObject IRuntimeMetaObjectProvider.GetMetaObject()
        {
            if (_runtime == null)
                _runtime = new RuntimeMetaObject(this);
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
                value = Data[locals[i].Index];
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
                var value = Data[locals[i].Index];
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

            int count = size;
            LocalVariable[] locals = this.locals;
            for (int i = 0; i < count; i++)
            {
                var local = locals[i];
                array[index++] = new KeyValuePair<string, object>(local.Name, Data[local.Index]);
            }
        }

        public bool Remove(string key)
        {
            throw new System.NotSupportedException("Remove not supported in dynamic object");
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new System.NotSupportedException("Remove not supported in dynamic object");
        }
        #endregion


    }
}