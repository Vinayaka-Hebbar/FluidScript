using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Runtime
{
    internal sealed class DynamicLocals : Collections.DictionaryBase<LocalVariable, object>, IDictionary<string, object>
    {
        static readonly IEqualityComparer<LocalVariable> DefaultComparer = EqualityComparer<LocalVariable>.Default;

        //keeps track of current locals
        private VariableIndexList current;

        internal DynamicLocals(int capacity) : base(capacity, DefaultComparer)
        {
            current = new VariableIndexList(null, capacity);
        }

        internal DynamicLocals() : base(0, DefaultComparer)
        {
            current = new VariableIndexList(null, 0);
        }

        public DynamicLocals(IDictionary<string, object> locals) : base(locals.Count, DefaultComparer)
        {
            current = new VariableIndexList(null, locals.Count);
            if (locals == null)
            {
                throw new System.ArgumentNullException(nameof(locals));
            }
            foreach (var item in locals)
            {
                Add(item.Key, item.Value);
            }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                int size = Count;
                string[] keys = new string[Count];
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
                int size = Count;
                object[] values = new object[size];
                for (int index = 0; index < size; index++)
                {
                    values[index] = entries[index].Value;
                }
                return values;
            }
        }

        public object this[string key]
        {
            get
            {
                var i = FindEntry(key);
                if (i >= 0)
                {
                    return entries[i].Value;
                }
                return null;
            }
            set
            {
                var i = FindEntry(key);
                if (i >= 0)
                {
                    entries[i].Value = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        internal void Create(string name, System.Type type, object value)
        {
            if (name == null)
            {
                throw new System.NullReferenceException(nameof(name));
            }
            var key = new LocalVariable(name, type);
            current.Store(Insert(key, value));
        }

        internal void InsertAtRoot(string name, System.Type type, object value)
        {
            if (name == null)
            {
                throw new System.NullReferenceException(nameof(name));
            }
            var key = new LocalVariable(name, type);
            current.StoreAtRoot(Insert(key, value));
        }

        internal int Insert(LocalVariable key, object value)
        {
            if (buckets == null) Initialize(0);
            int hashCode = key.GetHashCode();
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                {
                    throw new System.ArgumentException(string.Concat("Adding shadow variable ", key));
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
            key.Index = index;
            entries[index].HashCode = hashCode;
            entries[index].Next = buckets[targetBucket];
            entries[index].Key = key;
            entries[index].Value = value;
            buckets[targetBucket] = index;
            version++;
            return index;
        }

        internal void Update(LocalVariable key, object value)
        {
            int hashCode = key.GetHashCode();
            int targetBucket = hashCode % buckets.Length;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                {
                    entries[i].Value = value;
                    version++;
                    return;
                }
            }
            throw new System.Exception(string.Concat(key.Name, " not found in data"));
        }

        private int FindEntry(string key)
        {
            if (buckets != null)
            {
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Equals(entries[i].Key.Name, key)) return i;
                }
            }
            return -1;
        }

        internal System.IDisposable EnterScope()
        {
            return new LocalScope(this);
        }

        /// <summary>
        /// Get Member of current context and parent context
        /// </summary>
        internal bool TryLookVariable(string name, out LocalVariable variable)
        {
            var i = FindEntry(name);
            if (i >= 0)
            {
                variable = entries[i].Key;
                return true;
            }
            variable = LocalVariable.Empty;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        #region IDictionary
        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return FindEntry(key) >= 0;
        }

        public void Add(string key, object value)
        {
            var type = value == null ? Compiler.TypeProvider.ObjectType : value.GetType();
            Create(key, type, value);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new System.NotSupportedException("Remove not supported");
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            var i = FindEntry(key);
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

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotSupportedException("Remove not supported in dynamic object");
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        #endregion

        private struct LocalScope : System.IDisposable
        {
            private readonly DynamicLocals locals;

            public LocalScope(DynamicLocals locals) : this()
            {
                this.locals = locals;
                locals.current = new VariableIndexList(locals.current, 0);
            }

            public void Dispose()
            {
                var current = locals.current;
                var entires = locals.entries;
                foreach (var index in current.Entries)
                {
                    locals.Remove(entires[index].Key);
                }
                locals.current = current.parent;
            }
        }

        #region Enumerator
        [System.Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>,
            IDictionaryEnumerator
        {
            private readonly DynamicLocals dictionary;
            private readonly int version;
            private int index;
            private KeyValuePair<string, object> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            public const int DictEntry = 1;
            public const int KeyValuePair = 2;

            public Enumerator(DynamicLocals dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<string, object>();
            }

            public bool MoveNext()
            {
                if (version != dictionary.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)dictionary.count)
                {
                    if (dictionary.entries[index].HashCode >= 0)
                    {
                        current = new KeyValuePair<string, object>(dictionary.entries[index].Key.Name, dictionary.entries[index].Value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = dictionary.count + 1;
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
                    if (index == 0 || (index == dictionary.count + 1))
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
                if (version != dictionary.version)
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
                    if (index == 0 || (index == dictionary.count + 1))
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
                    if (index == 0 || (index == dictionary.count + 1))
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
                    if (index == 0 || (index == dictionary.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return current.Value;
                }
            }
        }
        #endregion

    }
}
