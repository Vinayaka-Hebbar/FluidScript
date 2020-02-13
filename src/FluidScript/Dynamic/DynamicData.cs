﻿using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    [System.Serializable]
    internal sealed class DynamicData : ICollection<LocalVariable>
    {
        static readonly IEqualityComparer<LocalVariable> DefaultComparer = EqualityComparer<LocalVariable>.Default;

        private struct Entry
        {
            public int HashCode;    // Lower 31 bits of hash code, -1 if unused
            public int Next;        // Index of next entry, -1 if last
            public LocalVariable Key;           // Key of entry
            public object Value;         // Value of entry
        }

        private Entry[] entries;

        private int[] buckets;
        private int count;
        private int version;
        private int freeList;
        private int freeCount;

        [System.NonSerialized]
        internal readonly object Instance;

        //todo remove instance move to locals
        internal DynamicData(object instance) : this(0)
        {
            Instance = instance;
        }

        internal DynamicData(int capacity)
        {
            if (capacity < 0) throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);
            Comparer = DefaultComparer;
        }

        internal DynamicData(DynamicData other) : this(other.count)
        {
            Instance = other.Instance;
            if(other == null)
                throw new System.ArgumentNullException(nameof(other));
            foreach (Entry entry in other.entries)
            {
                Insert(entry.Key, entry.Value);
            }
        }

        public IEqualityComparer<LocalVariable> Comparer { get; }


        private void Initialize(int capacity)
        {
            int size = Utils.DictionaryHelpers.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }

        #region List
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public object this[int index]
        {
            get
            {
                if (index >= 0) return entries[index].Value;
                throw new System.IndexOutOfRangeException(index.ToString());
            }
            set
            {
                entries[index].Value = value;
                version++;
            }
        }

        /// <summary>
        /// Read-only property describing how many elements are in the List.
        /// </summary>
        public int Count
        {
            get
            {
                return count - freeCount;
            }
        }

        internal LocalVariable Create(string name, System.Type type, object value)
        {
            if (name == null)
            {
                throw new System.NullReferenceException(nameof(name));
            }
            var key = new LocalVariable(name, type);
            return Insert(key, value);
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
            throw new System.Exception(string.Concat(key.Name, " not present in data"));
        }


        private LocalVariable Insert(LocalVariable key, object value)
        {
            if (buckets == null) Initialize(0);
            int hashCode = key.GetHashCode();
            int targetBucket = hashCode % buckets.Length;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                {
                    throw new System.ArgumentException(string.Concat("Adding Duplicate name ", key));
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
                    Resize(Utils.DictionaryHelpers.ExpandPrime(count), false);
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
            return key;
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

        public bool IsReadOnly => false;

        #endregion

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

        /// <summary>
        /// Get Member of current context and parent context
        /// </summary>
        internal bool TryLookVariable(string name, out LocalVariable variable)
        {
            var i = FindEntry(name);
            if(i >= 0)
            {
                variable = entries[i].Key;
                return true;
            }
            variable = LocalVariable.Empty;
            return false;
        }

        public void Clear()
        {
            if (count > 0)
            {
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                System.Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
                version++;
            }
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

            if (buckets != null)
            {
                int hashCode = key.GetHashCode();
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

        public bool Contains(LocalVariable obj)
        {
            return Exists(v => v.Equals(obj));
        }

        public bool Exists(System.Predicate<LocalVariable> match)
        {
            return FindIndex(0, count, match) != -1;
        }

        private int FindIndex(int startIndex, int count, System.Predicate<LocalVariable> match)
        {
            if ((uint)startIndex > (uint)count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (count < 0 || startIndex > count - count)
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
                if (match(entries[i].Key)) return i;
            }
            return -1;
        }

        void ICollection<LocalVariable>.Add(LocalVariable item)
        {
            Insert(item, null);
        }

        void ICollection<LocalVariable>.CopyTo(LocalVariable[] array, int arrayIndex)
        {
            // Delegate rest of error checking to Array.Copy.
            System.Array.Copy(entries, 0, array, arrayIndex, count);
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
        [System.Serializable]
        public struct Enumerator : IEnumerator, IEnumerator<LocalVariable>
        {
            private readonly DynamicData data;
            private int index;
            private readonly int version;
            private LocalVariable currentKey;

            internal Enumerator(DynamicData data)
            {
                this.data = data;
                version = data.version;
                index = 0;
                currentKey = LocalVariable.Empty;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (version != data.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                while ((uint)index < (uint)data.count)
                {
                    if (data.entries[index].HashCode >= 0)
                    {
                        currentKey = data.entries[index].Key;
                        index++;
                        return true;
                    }
                    index++;
                }

                index = data.count + 1;
                currentKey = LocalVariable.Empty;
                return false;
            }

            public LocalVariable Current
            {
                get
                {
                    return currentKey;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == data.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return currentKey;
                }
            }

            void IEnumerator.Reset()
            {
                if (version != data.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                index = 0;
                currentKey = LocalVariable.Empty;
            }
        }


    }
}
