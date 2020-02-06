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
        static readonly IEqualityComparer<LocalVariable> DefaultComparer = EqualityComparer<LocalVariable>.Default;

        private struct Entry
        {
            public int HashCode;    // Lower 31 bits of hash code, -1 if unused
            public int Next;        // Index of next entry, -1 if last
            public LocalVariable Key;           // Key of entry
            public object Value;         // Value of entry
        }

        private int[] buckets;
        private Entry[] entries;
        private int count;
        private int version;
        private int freeList;
        private int freeCount;

        private readonly DynamicClass _class;

        internal readonly DynamicObject Parent;

        private DynamicObject(int capacity)
        {
            if (capacity < 0) throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);
            Comparer = DefaultComparer;
        }

        internal DynamicObject(DynamicClass dclass) : this(0)
        {
            _class = dclass;
        }

        public DynamicObject() : this(0)
        {
            _class = new DynamicClass();
        }

        internal DynamicObject(DynamicClass obj, DynamicObject parent) : this(0)
        {
            _class = obj;
            Parent = parent;
        }

        #region Dicitonary
        public ICollection<string> Keys
        {
            get
            {
                string[] keys = new string[count];
                for (int index = 0; index < count; index++)
                {
                    var entry = entries[index];
                    if (entry.HashCode >= 0)
                        keys[index] = entry.Key.Name;
                }
                return keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                object[] values = new object[count];
                for (int index = 0; index < count; index++)
                {
                    var entry = entries[index];
                    if (entry.HashCode >= 0)
                        values[index] = entry.Value;
                }
                return values;
            }
        }

        public object this[LocalVariable key]
        {
            get
            {
                int i = FindEntry(key);
                if (i >= 0) return entries[i].Value;
                return null;
            }
            set
            {
                Insert(key, value, false);
            }
        }
        /// <summary>
        /// Getter and Setter of variables
        /// </summary>
        public object this[string name]
        {
            get
            {
                var i = FindFirstEntry(name);
                if (i >= 0) return entries[i].Value;
                return null;
            }
            set
            {
                Insert(name, value);
            }
        }

        public IEqualityComparer<LocalVariable> Comparer { get; }

        [Runtime.Register("count")]
        public int Count
        {
            get { return count - freeCount; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        private void Initialize(int capacity)
        {
            int size = Helpers.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }

        private int FindEntry(LocalVariable key)
        {
            if (key.Index < 0)
            {
                throw new KeyNotFoundException(nameof(key));
            }

            if (buckets != null)
            {
                int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key)) return i;
                }
            }
            return -1;
        }

        private int FindFirstEntry(string key)
        {
            Entry[] entries = this.entries;
            for (int index = 0; index < count; index++)
            {
                var entry = entries[index];
                if (entry.HashCode >= 0 && entry.Key.Equals(key))
                    return index;
            }
            return -1;
        }

        [Runtime.Register("contains")]
        public bool ContainsKey(string key)
        {
            return FindFirstEntry(key) >= 0;
        }

        public bool ContainsKey(LocalVariable key)
        {
            return FindEntry(key) >= 0;
        }

        public bool TryGetValue(LocalVariable key, out object value)
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

        /// <summary>
        /// Get current context variabke
        /// </summary>
        public bool TryGetMember(string key, out LocalVariable value)
        {
            int i = FindFirstEntry(key);
            if (i >= 0)
            {
                value = entries[i].Key;
                return true;
            }
            value = LocalVariable.Empty;
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            int i = FindFirstEntry(key);
            if (i >= 0)
            {
                value = entries[i].Value;
                return true;
            }
            value = null;
            return false;
        }

        internal IEnumerable<object> FindValues(string name, System.Func<LocalVariable, bool> predicat)
        {
            for (int index = 0; index < count; index++)
            {
                var item = entries[index];
                if (item.HashCode >=0 && item.Key.Equals(name) && predicat(item.Key))
                {
                    yield return item.Value;
                }
            }
        }

        internal void Insert(LocalVariable key, object value, bool add)
        {
            if (key.Index < 0)
            {
                throw new KeyNotFoundException(nameof(key));
            }
            if (buckets == null) Initialize(0);
            int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                {
                    if (add)
                    {
                        throw new System.ArgumentException("Adding Duplicate");
                    }
                    entries[i].Value = value;
                    version++;
                    return;
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

            entries[index].HashCode = hashCode;
            entries[index].Next = buckets[targetBucket];
            entries[index].Key = key;
            entries[index].Value = value;
            buckets[targetBucket] = index;
            version++;
        }

        internal void Insert(string name, object value)
        {
            int i = FindFirstEntry(name);
            if (i >= 0)
            {
                Insert(entries[i].Key, value, false);
            }
            else
            {
                // value not created
                var variable = _class.Create(name, value == null ? Utils.TypeUtils.ObjectType : value.GetType());
                Insert(variable, value, true);
            }
        }

        internal void Add(string name, System.Type type, object value)
        {
            var variable = _class.Create(name, type);
            Insert(variable, value, true);
        }

        /// <summary>
        /// Remove item in hierarchy
        /// </summary>
        public bool Remove(LocalVariable key)
        {
            if (key.Index < 0)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            var result = InternalRemove(key);
            if (result == false && Parent != null)
                return Parent.Remove(key);
            return result;
        }

        private bool InternalRemove(LocalVariable key)
        {
            if (buckets != null)
            {
                int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucket = hashCode % buckets.Length;
                int last = -1;
                for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                    {
                        if (last < 0)
                        {
                            buckets[bucket] = entries[i].Next;
                        }
                        else
                        {
                            entries[last].Next = entries[i].Next;
                        }
                        entries[i].HashCode = -1;
                        entries[i].Next = freeList;
                        entries[i].Key = LocalVariable.Empty;
                        entries[i].Value = null;
                        freeList = i;
                        freeCount++;
                        version++;
                        return true;
                    }
                }
            }
            return false;
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            //Contract.Assert(newSize >= entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            System.Array.Copy(entries, 0, newEntries, 0, count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < count; i++)
                {
                    if (newEntries[i].HashCode != -1)
                    {
                        newEntries[i].HashCode = (Comparer.GetHashCode(newEntries[i].Key) & 0x7FFFFFFF);
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (newEntries[i].HashCode >= 0)
                {
                    int bucket = newEntries[i].HashCode % newSize;
                    newEntries[i].Next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            entries = newEntries;
        }

        [Runtime.Register("clear")]
        public void Clear()
        {
            if (count > 0)
            {
                Detach();
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                System.Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
                version++;
            }
        }

        #endregion

        /// <summary>
        /// if exists in current context else in parent
        /// </summary>
        internal void Modify(LocalVariable variable, object value)
        {
            if (FindEntry(variable) < 0 && Parent != null)
            {
                Parent.Modify(variable, value);
                return;
            }
            Insert(variable, value, false);
        }

        /// <summary>
        /// Find the variable in the hierarchy
        /// </summary>
        /// <param name="name">name to find</param>
        /// <returns>value</returns>
        internal object Find(string name)
        {
            var i = FindFirstEntry(name);
            if (i >= 0)
            {
                return entries[i].Value;
            }
            return Parent?.Find(name);
        }

        /// <summary>
        /// Detaches variables from instance
        /// </summary>
        internal void Detach()
        {
            int index = count - 1;
            Entry[] entries = this.entries;
            for (; index > -1; index--)
            {
                var entry = entries[index];
                if (entry.HashCode >= 0)
                    _class.Remove(entry.Key);
            }
        }

        internal bool TryFind(LocalVariable item, out object value)
        {
            int i = FindEntry(item);
            if (i >= 0)
            {
                value = entries[i].Value;
                return true;
            }
            if (Parent != null)
            {
                return Parent.TryFind(item, out value);
            }
            value = null;
            return false;
        }

        internal object GetValue(LocalVariable item)
        {
            int i = FindEntry(item);
            if (i >= 0)
            {
                return entries[i].Value;
            }
            return Parent?.GetValue(item);
        }

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
            var keys = new Collections.List<String>(count);
            for (int index = 0; index < count; index++)
            {
                var entry = entries[index];
                if (entry.HashCode >= 0)
                    keys.Add(entry.Key.Name);
            }
            return keys;
        }
        #endregion

        #region DictionaryEnumerator

        [System.Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IDictionaryEnumerator
        {
            private readonly DynamicObject context;
            private readonly int version;
            private int index;
            private KeyValuePair<string, object> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(DynamicObject context, int getEnumeratorRetType)
            {
                this.context = context;
                version = context.version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<string, object>();
            }

            public bool MoveNext()
            {
                if (version != context.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)context.count)
                {
                    if (context.entries[index].HashCode >= 0)
                    {
                        current = new KeyValuePair<string, object>(context.entries[index].Key.Name, context.entries[index].Value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = context.count + 1;
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
                    if (index == 0 || (index == context.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new System.Collections.DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<string, object>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != context.version)
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
                    if (index == 0 || (index == context.count + 1))
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
                    if (index == 0 || (index == context.count + 1))
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
                    if (index == 0 || (index == context.count + 1))
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
            //todo list serialize
            for (int index = 0; index < count; index++)
            {
                var entry = entries[index];
                if (entry.HashCode >= 0)
                {
                    var item = entry.Key;
                    var value = entry.Value;
                    if (item.Type.IsPrimitive)
                        info.AddValue(item.Name, value, item.Type);
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

        void IDictionary<string, object>.Add(string key, object value)
        {
            Insert(key, value);
        }

        public bool Remove(string key)
        {
            var i = FindFirstEntry(key);
            if (i >= 0)
            {
                return Remove(entries[i].Key);
            }
            return false;
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            int i = FindFirstEntry(key);
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
            Insert(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            int i = FindFirstEntry(item.Key);
            if (i >= 0 && EqualityComparer<object>.Default.Equals(entries[i].Value, item.Value))
            {
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

            int count = this.count;
            Entry[] entries = this.entries;
            for (int i = 0; i < count; i++)
            {
                if (entries[i].HashCode >= 0)
                {
                    array[index++] = new KeyValuePair<string, object>(entries[i].Key.Name, entries[i].Value);
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            int i = FindFirstEntry(item.Key);
            if (i >= 0 && EqualityComparer<object>.Default.Equals(entries[i].Value, item.Value))
            {
                Remove(entries[i].Key);
                return true;
            }
            return false;
        }
        #endregion

        internal static class Helpers
        {
            public static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

            internal const int HashPrime = 101;

            public const int MaxPrimeArrayLength = 0x7FEFFFFD;

            public static int GetPrime(int min)
            {
                if (min < 0)
                    throw new System.ArgumentException();
                // Contract.EndContractBlock();

                for (int i = 0; i < primes.Length; i++)
                {
                    int prime = primes[i];
                    if (prime >= min) return prime;
                }

                //outside of our predefined table. 
                //compute the hard way. 
                for (int i = (min | 1); i < int.MaxValue; i += 2)
                {
                    if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                        return i;
                }
                return min;
            }

            public static bool IsPrime(int candidate)
            {
                if ((candidate & 1) != 0)
                {
                    int limit = (int)Math.Sqrt(candidate);
                    for (int divisor = 3; divisor <= limit; divisor += 2)
                    {
                        if ((candidate % divisor) == 0)
                            return false;
                    }
                    return true;
                }
                return (candidate == 2);
            }

            public static int ExpandPrime(int oldSize)
            {
                int newSize = 2 * oldSize;

                // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
                {
                    System.Diagnostics.Contracts.Contract.Assert(MaxPrimeArrayLength == GetPrime(MaxPrimeArrayLength), "Invalid MaxPrimeArrayLength");
                    return MaxPrimeArrayLength;
                }

                return GetPrime(newSize);
            }
        }
    }
}