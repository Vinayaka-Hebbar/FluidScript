namespace FluidScript.Compiler.Binders
{
    public sealed class ArgumentConversions
    {
        Conversion[] items;
        int count;
        int start;

        public ArgumentConversions(int capacity)
        {
            items = new Conversion[capacity];
        }

        public int Count => count;

        public void Add(Conversion conversion)
        {
            Insert(conversion.Index, conversion);
        }

        internal void Insert(int index, Conversion c)
        {
            if(index < count)
            {
                // add to next node
                Conversion last = items[index];
                c.next = last;
                last.next = c;
                items[index] = c;
                return;
            }
            if (count == items.Length)
            {
                Resize(2 * count);
            }
            /// empty next node
            c.next = c;
            items[count] = c;
            count++;
        }

        internal Conversion this[int index]
        {
            get
            {
                if (count > index)
                {
                    return items[index];
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

        private void Resize(int newSize)
        {
            var newItems = new Conversion[newSize];
            System.Array.Copy(items, 0, newItems, 0, count);
            items = newItems;
        }

        public void Invoke(ref object[] values)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
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
