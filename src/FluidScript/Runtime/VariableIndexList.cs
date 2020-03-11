namespace FluidScript.Runtime
{
    internal sealed class VariableIndexList
    {
        private int[] indexes;
        internal int size;

        internal VariableIndexList parent;

        public VariableIndexList(VariableIndexList parent, int capacity)
        {
            this.parent = parent;
            if (capacity < 0)
                throw new System.ArgumentOutOfRangeException(nameof(capacity));
            if (capacity == 0)
                indexes = new int[2];
            else
                indexes = new int[capacity];
        }

        public void StoreAtRoot(int index)
        {
            if (parent != null)
                parent.StoreAtRoot(index);
            else
                Store(index);
        }

        public void Store(int index)
        {
            if (size == indexes.Length)
            {
                // checked for overflow of integer
                int[] newItems = new int[checked(size * 2)];
                System.Array.Copy(indexes, 0, newItems, 0, size);
                indexes = newItems;
            }
            indexes[size++] = index;
        }

        public System.Collections.Generic.IEnumerable<int> Entries
        {
            get
            {
                for (int i = size - 1; i > -1; i--)
                    yield return indexes[i];
            }
        }
    }
}
