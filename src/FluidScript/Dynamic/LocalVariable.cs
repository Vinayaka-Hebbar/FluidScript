
namespace FluidScript.Dynamic
{
    public struct LocalVariable : IFSObject, System.IEquatable<LocalVariable>
    {

        internal static readonly LocalVariable Empty = new LocalVariable(string.Empty, null);

        internal readonly string Name;
        internal int Index;
        internal int Next;
        internal readonly System.Type Type;
        internal readonly int HashToken;

        internal LocalVariable(string name, System.Type type)
        {
            Name = name;
            Type = type;
            HashToken = name.GetHashCode() & 0x7FFFFFFF;
            Index = Next = -1;
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
            return HashToken;
        }

        [Runtime.Register("hashCode")]
        public Integer HashCode()
        {
            return new Integer(HashToken);
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

        public bool Equals(LocalVariable other)
        {
            return Name == other.Name && other.Index == Index;
        }
    }
}