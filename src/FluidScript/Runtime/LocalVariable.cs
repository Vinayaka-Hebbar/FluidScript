
namespace FluidScript.Runtime
{
    public
#if LATEST_VS
        readonly
#endif
        struct LocalVariable : Compiler.Emit.ILocalVariable, System.IEquatable<LocalVariable>
    {
        internal static readonly LocalVariable Empty = new LocalVariable(string.Empty, null, -1, -1);

        public string Name { get; }

        public int Index { get; }

        public System.Type Type { get; }

        readonly int hashCode;

        internal LocalVariable(string name, System.Type type, int index, int hashCode)
        {
            Name = name;
            Type = type;
            Index = index;
            this.hashCode = hashCode;
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
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }

        public bool Equals(LocalVariable other)
        {
            return Name == other.Name;
        }
    }
}