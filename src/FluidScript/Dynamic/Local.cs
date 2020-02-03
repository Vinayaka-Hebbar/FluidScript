using System;
using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    public class Local : IEnumerable<KeyValuePair<LocalVariable, object>>
    {
        public struct Entry
        {
            public int HashCode;    // Lower 31 bits of hash code, -1 if unused
            public int Next;        // Index of next entry, -1 if last
            public LocalVariable Key;           // Key of entry
            public object Value;         // Value of entry
        }

        public struct NameEntry
        {
            public int HashCode;
            public int Next;
            public string Key;           // Key of entry
            public LocalVariable Value;
        }

        private int[] buckets;
        private int[] nameBuckets;
        private Entry[] entries;
        private NameEntry[] names;
        private int count;
        private int version;
        private int freeList;
        private int freeCount;
        private KeyCollection keys;
        private object _syncRoot;

        public Local() : this(0, null) { }

        public Local(int capacity) : this(capacity, null) { }

        public Local(int capacity, IEqualityComparer<LocalVariable> comparer)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);
            Comparer = comparer ?? EqualityComparer<LocalVariable>.Default;
        }

        public IReadOnlyCollection<Entry> Entries
        {
            get => entries;
        }

        public IReadOnlyCollection<NameEntry> Names
        {
            get => names;
        }

        public IEqualityComparer<LocalVariable> Comparer { get; }

        public IEqualityComparer<string> NameComparer { get; }

        public int Count
        {
            get { return count - freeCount; }
        }

        public KeyCollection Keys
        {
            get
            {
                //Contract.Ensures(Contract.Result<KeyCollection>() != null);
                if (keys == null) keys = new KeyCollection(this);
                return keys;
            }
        }

        public object this[LocalVariable key]
        {
            get
            {
                int i = FindEntry(key);
                if (i >= 0) return entries[i].Value;
                throw new KeyNotFoundException(key.ToString());
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public object this[string key]
        {
            get
            {
                int i = FindEntry(key);
                if (i >= 0) return entries[i].Value;
                throw new KeyNotFoundException(key.ToString());
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(LocalVariable key, object value)
        {
            Insert(key, value, true);
        }

        public void Clear()
        {
            if (count > 0)
            {
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
                version++;
            }
        }

        public bool ContainsKey(LocalVariable key)
        {
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(object value)
        {
            if (value is null)
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0 && entries[i].Value == null) return true;
                }
            }
            else
            {
                EqualityComparer<object> c = EqualityComparer<object>.Default;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0 && c.Equals(entries[i].Value, value)) return true;
                }
            }
            return false;
        }

        private void CopyTo(KeyValuePair<LocalVariable, object>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(string.Concat("Index ", index, " out of range"));
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("array too small");
            }

            int count = this.count;
            Entry[] entries = this.entries;
            for (int i = 0; i < count; i++)
            {
                if (entries[i].HashCode >= 0)
                {
                    array[index++] = new KeyValuePair<LocalVariable, object>(entries[i].Key, entries[i].Value);
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<LocalVariable, object>> IEnumerable<KeyValuePair<LocalVariable, object>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        private int FindEntry(LocalVariable key)
        {
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

        private int FindEntry(string key)
        {
            if (buckets != null)
            {
                int hashCode = NameComparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && NameComparer.Equals(entries[i].Key, key)) return i;
                }
            }
            return -1;
        }

        private void Initialize(int capacity)
        {
            int size = Helpers.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }

        private void Insert(LocalVariable key, object value, bool add)
        {
            if (buckets == null) Initialize(0);
            int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key))
                {
                    if (add)
                    {
                        throw new ArgumentException("Adding Duplicate");
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
                    Resize();
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

#if FEATURE_RANDOMIZED_STRING_HASHING

#if FEATURE_CORECLR
            // In case we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
            // in this case will be EqualityComparer<string>.Default.
            // Note, randomized string hashing is turned on by default on coreclr so EqualityComparer<string>.Default will 
            // be using randomized string hashing

            if (collisionCount > HashHelpers.HashCollisionThreshold && comparer == NonRandomizedStringEqualityComparer.Default) 
            {
                comparer = (IEqualityComparer<TKey>) EqualityComparer<string>.Default;
                Resize(entries.Length, true);
            }
#else
            if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(comparer)) 
            {
                comparer = (IEqualityComparer<TKey>) HashHelpers.GetRandomizedEqualityComparer(comparer);
                Resize(entries.Length, true);
            }
#endif // FEATURE_CORECLR

#endif

        }

        private void Resize()
        {
            Resize(Helpers.ExpandPrime(count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            //Contract.Assert(newSize >= entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
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

        public bool Remove(LocalVariable key)
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

        // This is a convenience method for the internal callers that were converted from using Hashtable.
        // Many were combining key doesn't exist and key exists but null value (for non-value types) checks.
        // This allows them to continue getting that behavior with minimal code delta. This is basically
        // TryGetValue without the out param
        internal object GetValueOrDefault(LocalVariable key)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                return entries[i].Value;
            }
            return null;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<LocalVariable, object>>,
            IDictionaryEnumerator
        {
            private readonly Local dictionary;
            private int version;
            private int index;
            private KeyValuePair<LocalVariable, object> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(Local dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<LocalVariable, object>();
            }

            public bool MoveNext()
            {
                if (version != dictionary.version)
                {
                    throw new InvalidOperationException(nameof(version));
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)dictionary.count)
                {
                    if (dictionary.entries[index].HashCode >= 0)
                    {
                        current = new KeyValuePair<LocalVariable, object>(dictionary.entries[index].Key, dictionary.entries[index].Value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = dictionary.count + 1;
                current = new KeyValuePair<LocalVariable, object>();
                return false;
            }

            public KeyValuePair<LocalVariable, object> Current
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
                        throw new InvalidOperationException("Operation can't happen");
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new System.Collections.DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<LocalVariable, object>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != dictionary.version)
                {
                    throw new InvalidOperationException(nameof(version));
                }

                index = 0;
                current = new KeyValuePair<LocalVariable, object>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (index == 0 || (index == dictionary.count + 1))
                    {
                        throw new InvalidOperationException("Operation can't happen");
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
                        throw new InvalidOperationException("Operation can't happen");
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
                        throw new InvalidOperationException("Operation can't happen");
                    }

                    return current.Value;
                }
            }
        }

        [Serializable]
        public sealed class KeyCollection : ICollection<LocalVariable>, ICollection, IReadOnlyCollection<LocalVariable>
        {
            private readonly Local dictionary;

            public KeyCollection(Local dictionary)
            {
                this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            public void CopyTo(LocalVariable[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(array));
                }

                if (array.Length - index < dictionary.Count)
                {
                    throw new ArgumentException("Array of too small");
                }

                int count = dictionary.count;
                Entry[] entries = dictionary.entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].HashCode >= 0) array[index++] = entries[i].Key;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<LocalVariable>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<LocalVariable>.Add(LocalVariable item)
            {
                throw new NotSupportedException(nameof(Clear));
            }

            void ICollection<LocalVariable>.Clear()
            {
                throw new NotSupportedException(nameof(Clear));
            }

            bool ICollection<LocalVariable>.Contains(LocalVariable item)
            {
                return dictionary.ContainsKey(item);
            }

            bool ICollection<LocalVariable>.Remove(LocalVariable item)
            {
                throw new NotSupportedException(nameof(Remove));
            }

            IEnumerator<LocalVariable> IEnumerable<LocalVariable>.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException("Multi Dim not supported");
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentException("Non zero lower bound");
                }

                if (index < 0 || index > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(array));
                }

                if (array.Length - index < dictionary.Count)
                {
                    throw new ArgumentException("Array of too small");
                }

                LocalVariable[] keys = array as LocalVariable[];
                if (keys != null)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArgumentException("Invalid array type");
                    }

                    int count = dictionary.count;
                    Entry[] entries = dictionary.entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].HashCode >= 0) objects[index++] = entries[i].Key;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw;
                    }
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            Object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }


            [Serializable]
            public struct Enumerator : IEnumerator<LocalVariable>, System.Collections.IEnumerator
            {
                private Local dictionary;
                private int index;
                private int version;
                private LocalVariable currentKey;

                internal Enumerator(Local dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = 0;
                    currentKey = LocalVariable.Empty;
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException(nameof(version));
                    }

                    while ((uint)index < (uint)dictionary.count)
                    {
                        if (dictionary.entries[index].HashCode >= 0)
                        {
                            currentKey = dictionary.entries[index].Key;
                            index++;
                            return true;
                        }
                        index++;
                    }

                    index = dictionary.count + 1;
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

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        if (index == 0 || (index == dictionary.count + 1))
                        {
                            throw new InvalidOperationException("Operation can't happen");
                        }

                        return currentKey;
                    }
                }

                void System.Collections.IEnumerator.Reset()
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException(nameof(version));
                    }

                    index = 0;
                    currentKey = LocalVariable.Empty;
                }
            }
        }

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
                    throw new ArgumentException();
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
