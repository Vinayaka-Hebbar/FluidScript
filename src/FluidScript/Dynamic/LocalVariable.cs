namespace FluidScript.Dynamic
{
    public struct LocalVariable : IFSObject
    {
        /// <summary>
        /// calculated value
        /// </summary>
        private const int HashCodeInit = 2063038313;
        private const int HashMultiplier = -1521134295;

        internal static readonly LocalVariable Empty = new LocalVariable(string.Empty, -1, null);

        internal readonly string Name;
        internal readonly int Index;
        internal readonly System.Type Type;
        internal readonly int HashValue;


        public LocalVariable(string name, int index, System.Type type)
        {
            Name = name;
            Index = index;
            Type = type;
            HashValue = ((HashCodeInit + name.GetHashCode()) * HashMultiplier) + index;
        }

        public override bool Equals(object obj)
        {
            if (obj is LocalVariable other)
            {
                if (Name == null && other.Name == null)
                    return true;
                return other.Index == Index && Name.Equals(other.Name);
            }
            return Name.Equals(obj);
        }

        public bool Equals(string obj)
        {
            return Name.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashValue;
        }

        [Runtime.Register("hashCode")]
        public Integer HashCode()
        {
            return new Integer(HashValue);
        }

        public override string ToString() => string.Concat(Name, ":", Type.Name);

        [Runtime.Register("equals")]
        public Boolean __Equals(IFSObject obj)
        {
            return Equals(obj) ? Boolean.True : Boolean.False;
        }

        [Runtime.Register("toString")]
        public String __ToString()
        {
            return new String(string.Concat(Name, ":", Type.Name));
        }
    }
}