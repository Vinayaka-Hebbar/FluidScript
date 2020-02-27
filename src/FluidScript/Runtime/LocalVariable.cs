
namespace FluidScript.Runtime
{
    public struct LocalVariable : IFSObject, System.IEquatable<LocalVariable>
    {
        internal static readonly LocalVariable Empty = new LocalVariable(string.Empty, null);

        internal readonly string Name;
        internal int Index;
        internal int Next;
        internal readonly System.Type Type;
        readonly int _hashCode;

        internal LocalVariable(string name, System.Type type)
        {
            Name = name;
            Type = type;
            _hashCode = name.GetHashCode() & 0x7FFFFFFF;
            Index = Next = -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is LocalVariable other)
            {
                if (Name == null && other.Name == null)
                    return true;
                return Name.Equals(other.Name);
            }
            return Name.Equals(obj);
        }

        public bool Equals(string obj)
        {
            return Name.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        [Runtime.Register("hashCode")]
        public Integer __HashCode()
        {
            return new Integer(_hashCode);
        }

        public override string ToString() => string.Concat(Name, ":", Type.Name);

        [Runtime.Register("equals")]
        public Boolean Equals(IFSObject obj)
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
            return Name == other.Name;
        }
    }
}