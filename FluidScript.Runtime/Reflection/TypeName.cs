namespace FluidScript.Reflection
{
    public struct TypeName
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

        internal static TypeName Split(string name)
        {
            int dot = name.LastIndexOf('.');
            if (dot == -1)
            {
                return new TypeName(null, name);
            }
            else
            {
                return new TypeName(name.Substring(0, dot), name.Substring(dot + 1));
            }
        }
    }
}
