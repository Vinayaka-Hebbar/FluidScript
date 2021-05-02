namespace FluidScript.Collections
{
    public interface ICollection<T> : IEnumerable<T>
    {
        [Runtime.Register("count")]
        // Number of items in the collections.        
        int Count { get; }

        [Runtime.Register("isReadOnly")]
        bool IsReadOnly { get; }

        [Runtime.Register("add")]
        void Add(T item);

        [Runtime.Register("clear")]
        void Clear();

        [Runtime.Register("contains")]
        bool Contains(T item);

        // CopyTo copies a collection into an Array, starting at a particular
        // index into the array.
        // 
        void CopyTo(T[] array, int arrayIndex);

        //void CopyTo(int sourceIndex, T[] destinationArray, int destinationIndex, int count);

        [Runtime.Register("remove")]
        bool Remove(T item);
    }
}
