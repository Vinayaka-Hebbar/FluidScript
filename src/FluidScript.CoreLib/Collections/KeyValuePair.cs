﻿namespace FluidScript.Collections
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
        Integer IFSObject.GetHashCode()
        {
            return GetHashCode();
        }

        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
        {
            return Equals(obj);
        }

        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return
                new String(string.Concat("[", key != null ? key.ToString() : string.Empty, ", ", value != null ? value.ToString() : string.Empty, "]"));
        }
    }
}
