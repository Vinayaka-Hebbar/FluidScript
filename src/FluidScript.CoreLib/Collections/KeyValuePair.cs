namespace FluidScript.Collections
{
    [System.Serializable]
    public
#if LATEST_VS
        readonly
#endif
        struct KeyValuePair<TKey, TValue> : IFSObject
    {
        private readonly TKey key;
        private readonly TValue value;

        public KeyValuePair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        [Runtime.Register("key")]
        public TKey Key
        {
            get { return key; }
        }

        [Runtime.Register("value")]
        public TValue Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            return string.Concat("[", key != null ? key.ToString() : string.Empty, ", ", value != null ? value.ToString() : string.Empty, "]");
        }

        [Runtime.Register("hashCode")]
        Integer IFSObject.HashCode()
        {
            return GetHashCode();
        }

        [Runtime.Register("equals")]
        Boolean IFSObject.__Equals(IFSObject obj)
        {
            return Equals(obj);
        }

        [Runtime.Register("toString")]
        String IFSObject.__ToString()
        {
            return
                new String(string.Concat("[", key != null ? key.ToString() : string.Empty, ", ", value != null ? value.ToString() : string.Empty, "]"));
        }
    }
}
