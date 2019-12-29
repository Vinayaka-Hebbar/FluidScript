namespace FluidScript.Collections
{
    [System.Serializable]
    public struct KeyValuePair<TKey, TValue> : IFSObject
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

        [Runtime.Register("hashCode")]
        public Integer HashCode()
        {
            return GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat("[", key != null ? key.ToString() : string.Empty, ", ", value != null ? value.ToString() : string.Empty, "]");
        }

        [Runtime.Register("equals")]
        public Boolean __Equals(IFSObject obj)
        {
            return Equals(obj);
        }

        [Runtime.Register("toString")]
        public String __ToString()
        {
            return 
                new String(string.Concat("[", key != null ? key.ToString() : string.Empty, ", ", value != null ? value.ToString() : string.Empty, "]"));
        }
    }
}
