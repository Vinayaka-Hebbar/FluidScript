namespace FluidScript.Runtime
{
    public sealed class ArgumentConversions
    {
        Conversion[] items;
        int limit;
        int start;

        public ArgumentConversions(int capacity)
        {
            items = new Conversion[capacity];
        }

        public void Add(Conversion conversion)
        {
            Append(conversion.Index, conversion);
        }

        public int Count => limit;

        public void Append(int index, Conversion c)
        {
            if (index == items.Length)
            {
                Resize(2 * index);
            }

            // add to next node
            Conversion last = items[index];
            if (last == null)
            {
                /// empty next node
                c.next = c;
                items[index] = c;
                limit = index + 1;
                return;
            }
            c.next = last;
            last.next = c;
            items[index] = c;
            return;
        }

        public Conversion this[int index]
        {
            get
            {
                if (index < limit)
                {
                    return items[index];
                }
                return null;
            }
        }

        /// <summary>
        /// Save the position
        /// </summary>
        public void Backup()
        {
            start = limit;
        }

        /// <summary>
        /// Revert back to last backup position <see cref="Backup"/>
        /// </summary>
        /// <returns></returns>
        public bool Recycle()
        {
            // Reset the array values length
            limit = start;
            return false;
        }

        private void Resize(int newSize)
        {
            var newItems = new Conversion[newSize];
            System.Array.Copy(items, 0, newItems, 0, items.Length);
            items = newItems;
        }

        public void Invoke(ref object[] values)
        {
            if (limit > 0)
            {
                for (int i = 0; i < limit; i++)
                {
                    var c = items[i];
                    if (c == null)
                        continue;
                    Conversion n = c;
                    do
                    {
                        n = n.next;
                        switch (n.ConversionType)
                        {
                            case ConversionType.Normal:
                                var arg = values[i];
                                values[i] = n.Invoke(arg);
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
