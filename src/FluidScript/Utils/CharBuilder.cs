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

        public unsafe void Append(char value)
        {
            if (size == items.Length)
            {
                int newCapacity = items.Length * 2;
                // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
                // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
                if (newCapacity > int.MaxValue) newCapacity = int.MaxValue;
                char[] newItems = new char[newCapacity];
                // srcPtr and destPtr are IntPtr's pointing to valid memory locations
                // size is the number of long (normally 4 bytes) to copy
                fixed (char* src = items, dest = newItems)
                    for (int i = 0; i < size; i++)
                        dest[i] = src[i];
                items = newItems;
            }
            items[size++] = value;
        }

        public override string ToString()
        {
            return new string(items, 0, (int)size);
        }
    }
}
