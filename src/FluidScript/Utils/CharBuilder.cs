namespace FluidScript.Utils
{
    public sealed class CharBuilder
    {
        const int DefaultCapacity = 16;
        private char[] items;
        private int size;

        public CharBuilder()
        {
            items = new char[DefaultCapacity];
        }

        public int Length
        {
            get => size;
            set
            {
                if (value == 0)
                {
                    size = 0;
                    return;
                }
                if (size < value)
                {
                    EnsureCapcity(value);
                }
            }
        }

        public void Append(char value)
        {
            if (size == items.Length)
            {
                EnsureCapcity(size + 1);
            }
            items[size++] = value;
        }

        private unsafe void EnsureCapcity(int min)
        {
            int newCapacity = items.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if (newCapacity > int.MaxValue)
                newCapacity = int.MaxValue;
            if (newCapacity < min)
                newCapacity = min;
            char[] newItems = new char[newCapacity];
            // srcPtr and destPtr are IntPtr's pointing to valid memory locations
            // size is the number of long (normally 4 bytes) to copy
            fixed (char* src = items, dest = newItems)
                for (int i = 0; i < size; i++)
                    dest[i] = src[i];
            items = newItems;
        }

        public override string ToString()
        {
            return new string(items, 0, (int)size);
        }
    }
}
