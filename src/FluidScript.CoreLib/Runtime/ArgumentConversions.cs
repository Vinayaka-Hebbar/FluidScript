namespace FluidScript.Runtime
{
    /// <summary>
    /// Conversion before passing arguments
    /// </summary>
    public sealed class ArgumentConversions
    {
        Conversion[] items;
        /// max argument index
        int size;

        /// <summary>
        /// Initalizes Conversions for arguments
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public ArgumentConversions(int capacity)
        {
            items = new Conversion[capacity];
        }

        public void Add(Conversion conversion)
        {
            Append(conversion.Index, conversion);
        }

        public void AddRange(int index, params Conversion[] conversions)
        {
            for (int i = 0; i < conversions.Length; i++)
            {
                Append(index, conversions[i]);
            }
        }

        /// <summary>
        /// Current conversion limit
        /// </summary>
        public int Count => size;

        public void Append(int index, Conversion c)
        {
            // checking array limit
            if (index == items.Length)
            {
                Resize(2 * index);
            }

            // last conversion for the index
            Conversion last = items[index];
            // should not cross the limit 
            if (last == null || index >= size)
            {
                // empty next node
                c.next = c;
                items[index] = c;
                // index should not be more
                size = index + 1;
                return;
            }

            // add to next node if exist
            c.next = last;
            last.next = c;
            items[index] = c;
            return;
        }

        public Conversion this[int index]
        {
            get
            {
                if (index < size)
                {
                    return items[index];
                }
                return null;
            }
        }

        /// <summary>
        /// Set Inital conversions
        /// </summary>
        public void SetInitial(Conversion[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                AddFirst(items[i]);
            }
        }

        public void AddFirst(Conversion item)
        {
            if (item != null)
            {
                var index = item.Index;
                if (index < size)
                {
                    var last = items[index];
                    if (last == null)
                    {
                        item.next = item;
                        items[index] = item;
                    }
                    else
                    {
                        item.next = last;
                        last.next = item;
                    }
                }
                else
                {
                    // deep clone required
                    Append(index, item);
                }
            }
        }

        /// <summary>
        /// Revert back to last backup position <see cref="SetInitial"/>
        /// </summary>
        /// <returns></returns>
        public bool Recycle()
        {
            // Reset the array values length if any backup
            size = 0;
            return false;
        }

        private void Resize(int newSize)
        {
            var newItems = new Conversion[newSize];
            System.Array.Copy(items, 0, newItems, 0, items.Length);
            items = newItems;
        }

        /// <summary>
        /// Convert arguments to pass to method
        /// </summary>
        /// <param name="values">Parametres to convert</param>
        public void Invoke(ref object[] values)
        {
            if (size > 0)
            {
                for (int i = 0; i < size; i++)
                {
                    var c = items[i];
                    // if conversion is null i.e, no conversions
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
