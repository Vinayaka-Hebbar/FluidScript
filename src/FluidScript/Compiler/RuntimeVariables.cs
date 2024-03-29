﻿using FluidScript.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Provides local variables for runtime
    /// </summary>
    public class RuntimeVariables : Collections.DictionaryBase<LocalVariable, object>, ILocalVariables
    {
        private static readonly IEqualityComparer<LocalVariable> DefaultComparer = EqualityComparer<LocalVariable>.Default;

        //keeps track of current locals
        VariableIndexList current;

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

        internal RuntimeVariables(int capacity) : base(capacity, DefaultComparer)
        {
            current = new VariableIndexList(null, capacity);
        }

        internal RuntimeVariables() : base(0, DefaultComparer)
        {
            current = new VariableIndexList(null, 0);
        }

        public RuntimeVariables(IDictionary<string, object> locals) : base(locals.Count, DefaultComparer)
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

        public object this[int index]
        {
            get
            {
                if (index >= 0 && index < count)
                    return entries[index].Value;
                throw new System.IndexOutOfRangeException(index.ToString());
            }
            set
            {
                if (index >= 0 && index < count)
                {
                    entries[index].Value = value;
                    version++;
                    return;
                }
                throw new System.IndexOutOfRangeException(index.ToString());
            }
        }

        public object this[string key]
        {
            get
            {
                var i = FindEntry(key);
                return i >= 0 ? entries[i].Value : null;
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

        public LocalVariable DeclareVariable(string name, System.Type type, object value)
        {
            if (name == null)
                throw new System.NullReferenceException(nameof(name));
            var key = Create(name, type, value);
            current.Store(key.Index);
            return key;
        }

        public LocalVariable DeclareVariable<T>(string name, T value = default(T))
        {
            if (name == null)
                throw new System.NullReferenceException(nameof(name));
            var key = Create(name, typeof(T), value);
            current.Store(key.Index);
            return key;
        }

        protected internal void InsertAtRoot(string name, System.Type type, object value)
        {
            if (name == null)
                throw new System.NullReferenceException(nameof(name));
            var key = Create(name, type, value);
            current.StoreAtRoot(key.Index);
        }

        internal LocalVariable Create(string name, System.Type type, object value)
        {
            if (buckets == null)
                Initialize(0);
            int hashCode = name.GetHashCode() & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && string.Equals(entries[i].Key.Name, name))
                {
                    throw new System.ArgumentException(string.Concat("Adding shadow variable '", name, '\''));
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
            version++;
            return variable;
        }

        protected int FindEntry(string key)
        {
            if (buckets != null)
            {
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && string.Equals(entries[i].Key.Name, key))
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get variable in the scope
        /// </summary>
        public virtual bool TryFindVariable(string name, out LocalVariable variable)
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

        internal
#if LATEST_VS
            readonly
#endif
            struct RuntimeScope : System.IDisposable
        {
            readonly RuntimeVariables locals;

            public RuntimeScope(RuntimeVariables locals)
            {
                this.locals = locals;
                locals.current = new VariableIndexList(locals.current, 0);
            }

            public void Dispose()
            {
                var current = locals.current;
                var entires = locals.entries;
                foreach (var index in current.Entries())
                {
                    locals.Remove(entires[index].Key);
                }
                locals.current = current.parent;
            }
        }

        #region IDictionary

        public void Add(string key, object value)
        {
            var type = value == null ? TypeProvider.ObjectType : value.GetType();
            DeclareVariable(key, type, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return FindEntry(key) >= 0;
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
            int i = FindEntry(item.Key);
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
            throw new System.NotSupportedException("Remove not supported");
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new System.NotSupportedException("Clear not supported");
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        #endregion

        #region Enumerator
        [System.Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>,
            IDictionaryEnumerator
        {
            private readonly RuntimeVariables dictionary;
            private readonly int version;
            private int index;
            private KeyValuePair<string, object> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            public const int DictEntry = 1;
            public const int KeyValuePair = 2;

            public Enumerator(RuntimeVariables dictionary, int getEnumeratorRetType)
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
