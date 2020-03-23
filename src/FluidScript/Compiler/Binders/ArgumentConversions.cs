using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Compiler.Binders
{
    public sealed class ArgumentConversions : IEnumerable
    {
        private struct Entry
        {
            public int Index;
            public Conversion Conversion;
            public int Next;
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

        internal void Insert(int index, Conversion conversion)
        {
            if (buckets == null) Initialize(3);
            int hashCode = index & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].Index == hashCode)
                {
                    // replace
                    entries[i].Conversion = conversion;
                    return;
                }
            }
            var target = count;
            if (count == entries.Length)
            {
                Resize(2 * count);
                targetBucket = hashCode % buckets.Length;
            }
            entries[target].Index = hashCode;
            entries[target].Conversion = conversion;
            entries[target].Next = buckets[targetBucket];
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
                    for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].Next)
                    {
                        if (entries[i].Index == hashCode)
                        {
                            var entry = entries[i];
                            return entry.Conversion;
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
            // Retail the array values ok
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
                if (newEntries[i].Index >= 0)
                {
                    int bucket = newEntries[i].Index % newSize;
                    newEntries[i].Next = newBuckets[bucket];
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
                    int index = entry.Index;
                    var convertion = entry.Conversion;
                    switch (convertion.ConversionType)
                    {
                        case ConversionType.Convert:
                            var arg = values[index];
                            values[index] = convertion.Invoke(arg);
                            break;
                        case ConversionType.ParamArray:
                            values = (object[])convertion.Invoke(values);
                            return;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        [System.Serializable]
        public struct Enumerator : IEnumerator
        {
            private readonly ArgumentConversions obj;
            private int index;
            private Conversion current;

            internal Enumerator(ArgumentConversions value)
            {
                obj = value;
                index = 0;
                current = null;
            }

            public bool MoveNext()
            {

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)obj.count)
                {
                    if (obj.entries[index].Index >= 0)
                    {
                        current = obj.entries[index].Conversion;
                        index++;
                        return true;
                    }
                    index++;
                }

                index = obj.count + 1;
                current = null;
                return false;
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

                    return current;
                }
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = null;
            }
        }
    }

    //public class ConversionGroup : System.Linq.IGrouping<int, Conversion>
    //{
    //    private int Size;

    //    private Conversion[] Conversions;
    //    public int Key { get; }


    //    public ConversionGroup(int index, Conversion[] conversions, int size)
    //    {
    //        Key = index;
    //        Conversions = conversions;
    //        Size = size;
    //    }

    //    internal void Add(Conversion conversion)
    //    {
    //        if (Size == Conversions.Length)
    //        {
    //            Conversion[] newItems = new Conversion[checked(Size * 2)];
    //            System.Array.Copy(Conversions, 0, newItems, 0, Size);
    //            Conversions = newItems;
    //        }
    //        Conversions[Size] = conversion;
    //        Size++;
    //    }

    //    internal void GenerateCode(Emit.MethodBodyGenerator generator)
    //    {
    //        for (int i = 0; i < Conversions.Length; i++)
    //        {
    //            Conversions[i].GenerateCode(generator);
    //        }
    //    }

    //    public void ForEach(System.Action<Conversion> predicate)
    //    {
    //        for (int i = 0; i < Conversions.Length; i++)
    //        {
    //            predicate(Conversions[i]);
    //        }
    //    }

    //    public IEnumerator<Conversion> GetEnumerator()
    //    {
    //        return new Enumerator(this);
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return new Enumerator(this);
    //    }

    //    [System.Serializable]
    //    public struct Enumerator : IEnumerator<Conversion>
    //    {
    //        private readonly ConversionGroup obj;
    //        private int index;
    //        private Conversion current;

    //        internal Enumerator(ConversionGroup value)
    //        {
    //            obj = value;
    //            index = 0;
    //            current = null;
    //        }

    //        public bool MoveNext()
    //        {
    //            // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
    //            // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
    //            while ((uint)index < (uint)obj.Size)
    //            {
    //                current = obj.Conversions[index++];
    //                return true;
    //            }
    //            index = obj.Size + 1;
    //            current = null;
    //            return false;
    //        }

    //        public Conversion Current
    //        {
    //            get { return current; }
    //        }

    //        public void Dispose()
    //        {
    //        }

    //        object IEnumerator.Current
    //        {
    //            get
    //            {
    //                if (index == 0 || (index == obj.Size + 1))
    //                {
    //                    throw new System.InvalidOperationException("Operation can't happen");
    //                }

    //                return obj.Conversions[index];
    //            }
    //        }

    //        void IEnumerator.Reset()
    //        {
    //            index = 0;
    //            current = null;
    //        }
    //    }
    //}
}
