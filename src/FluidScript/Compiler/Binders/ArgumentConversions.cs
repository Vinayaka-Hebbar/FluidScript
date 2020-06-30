namespace FluidScript.Compiler.Binders
{
    public sealed class ArgumentConversions
    {
        private struct Entry
        {
            public int index;
            public Conversion conversion;
            public int next;
        }

        Entry[] entries;
        int[] buckets;
        int count;
        int start;

        public ArgumentConversions(int capacity)
        {
            if (capacity == 0)
                Initialize(3);
            else
                Initialize(capacity);
        }

        public int Count => count;

        public void Add(Conversion conversion)
        {
            Insert(conversion.Index, conversion);
        }

        internal void Insert(int index, Conversion c)
        {
            if (buckets == null) Initialize(3);
            int hashCode = index & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next)
            {
                if (entries[i].index == hashCode)
                {
                    // add to next node
                    Conversion last = entries[i].conversion;
                    c.next = last;
                    last.next = c;
                    entries[i].conversion = c;
                    return;
                }
            }
            var target = count;
            if (count == entries.Length)
            {
                Resize(2 * count);
                targetBucket = hashCode % buckets.Length;
            }
            c.next = c;
            entries[target].index = hashCode;
            entries[target].conversion = c;
            entries[target].next = buckets[targetBucket];
            buckets[targetBucket] = target;
            count++;
        }

        internal Conversion this[int index]
        {
            get
            {
                if (buckets != null)
                {
                    int hashCode = index & 0x7FFFFFFF;
                    for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
                    {
                        if (entries[i].index == hashCode)
                        {
                            var entry = entries[i];
                            return entry.conversion;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Save the position
        /// </summary>
        internal void Backup()
        {
            start = count;
        }

        /// <summary>
        /// Revert back to last backup position <see cref="Backup"/>
        /// </summary>
        /// <returns></returns>
        public bool Recycle()
        {
            // Reset the array values length
            count = start;
            return false;
        }

        private void Initialize(int size)
        {
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
        }

        private void Resize(int newSize)
        {
            //Contract.Assert(newSize >= entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            var newEntries = new Entry[newSize];
            System.Array.Copy(entries, 0, newEntries, 0, count);
            for (int i = 0; i < count; i++)
            {
                if (newEntries[i].index >= 0)
                {
                    int bucket = newEntries[i].index % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            entries = newEntries;
        }

        public void Invoke(ref object[] values)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var entry = entries[i];
                    int index = entry.index;
                    Conversion c = entry.conversion;
                    Conversion n = c;
                    do
                    {
                        n = n.next;
                        switch (n.ConversionType)
                        {
                            case ConversionType.Convert:
                                var arg = values[index];
                                values[index] = n.Invoke(arg);
                                break;
                            case ConversionType.ParamArray:
                                values = (object[])n.Invoke(values);
                                return;
                        }
                    } while (n != c);
                }
            }
        }
    }
}
