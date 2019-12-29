namespace FluidScript.Reflection
{
    /// <summary>
    /// Full Type Name
    /// </summary>
    public
#if LATEST_VS
        readonly
#endif
        struct TypeName
    {
        public string FullName => Namespace == null ? Name : Namespace + "." + Name;

        public readonly string Namespace;
        public readonly string Name;

        public TypeName(string ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString()
        {
            return FullName;
        }

        public override bool Equals(object obj)
        {
            if(obj is TypeName typeName)
            {
                if (typeName.Namespace == null && Namespace == null)
                    return Name.Equals(typeName.Name);
                return FullName.Equals(typeName.FullName);
            }
            return Namespace == null && Name.Equals(obj);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public static implicit operator TypeName(string fullName)
        {
            int dot = fullName.LastIndexOf('.');
            if (dot == -1)
            {
                return new TypeName(null, fullName);
            }
            else
            {
                return new TypeName(fullName.Substring(0, dot), fullName.Substring(dot + 1));
            }
        }
    }
}
