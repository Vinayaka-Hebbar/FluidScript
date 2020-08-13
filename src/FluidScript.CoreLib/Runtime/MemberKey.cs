using System;

namespace FluidScript.Runtime
{
    public
#if LATEST_VS
        readonly
#endif
        struct MemberKey : IEquatable<MemberKey>
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly int Index;
        internal readonly int HashCode;

        internal MemberKey(string name, Type type, int index, int hashCode)
        {
            Name = name;
            Type = type;
            Index = index;
            HashCode = hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is MemberKey other)
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
            return HashCode;
        }

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }

        public bool Equals(MemberKey other)
        {
            return Name == other.Name;
        }
    }
}
