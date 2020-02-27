namespace FluidScript.Collections
{
    public abstract class DictionaryBase<TKey, TValue>
    {
        protected struct Entry
        {
            public int HashCode;    // Lower 31 bits of hash code, -1 if unused
            public int Next;        // Index of next entry, -1 if last
            public TKey Key;           // Key of entry
            public TValue Value;         // Value of entry
        }

        protected Entry[] entries;

        protected int[] buckets;
        protected int count;
        protected int version;
        protected int freeList;
        protected int freeCount;

        protected DictionaryBase(int capacity, System.Collections.Generic.IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0) throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);
            Comparer = comparer;
        }

        protected DictionaryBase()
        {

        }

        public bool IsReadOnly => false;

        public System.Collections.Generic.IEqualityComparer<TKey> Comparer { get; }

        protected void Initialize(int capacity)
        {
            int size = Helpers.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }

        public TValue this[int index]
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

        public int Count
        {
            get
            {
                return count - freeCount;
            }
        }

        protected void Resize(int newSize, bool forceNewHashCodes)
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

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (buckets != null)
            {
                var entries = this.entries;
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
                        entries[i].Key = default(TKey);
                        entries[i].Value = default(TValue);
                        freeList = i;
                        freeCount++;
                        version++;
                        return true;
                    }
                }
            }
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

        protected int FindEntry(TKey key)
        {
            if (buckets != null)
            {
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                {
                    if (entries[i].HashCode == hashCode && Comparer.Equals(entries[i].Key, key)) return i;
                }
            }
            return -1;
        }

        protected static class Helpers
        {
            public static readonly int[] Primes = {
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

                for (int i = 0; i < Primes.Length; i++)
                {
                    int prime = Primes[i];
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
