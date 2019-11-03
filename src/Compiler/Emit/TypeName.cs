namespace FluidScript.Compiler.Emit
{
    public struct TypeName
    {
        public readonly static TypeName Empty = new TypeName();
        public readonly string FullName;
        public Reflection.DeclaredFlags Flags;

        public TypeName(string value)
        {
            FullName = value;
            Flags = Reflection.DeclaredFlags.None;
        }

        public TypeName(System.Type type)
        {
            FullName = type.FullName;
            Flags = type.IsArray ? Reflection.DeclaredFlags.Array : Reflection.DeclaredFlags.None;
        }

        public TypeName(string value, Reflection.DeclaredFlags flags)
        {
            FullName = value;
            Flags = flags;
        }

        public bool IsArray()
        {
            return (Flags & Reflection.DeclaredFlags.Array) == Reflection.DeclaredFlags.Array;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
