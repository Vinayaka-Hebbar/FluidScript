namespace FluidScript.Compiler.Emit
{
    public struct TypeName
    {
        public readonly static TypeName Any = new TypeName("any", Reflection.ArgumentFlags.None);
        public readonly string FullName;
        public Reflection.ArgumentFlags Flags;

        public TypeName(string value)
        {
            FullName = value;
            Flags = Reflection.ArgumentFlags.None;
        }

        public TypeName(System.Type type)
        {
            FullName = type.FullName;
            Flags = type.IsArray ? Reflection.ArgumentFlags.Array : Reflection.ArgumentFlags.None;
        }

        public TypeName(string value, Reflection.ArgumentFlags flags)
        {
            FullName = value;
            Flags = flags;
        }

        public bool IsArray()
        {
            return (Flags & Reflection.ArgumentFlags.Array) == Reflection.ArgumentFlags.Array;
        }

        internal RuntimeType GetRuntimeType()
        {
            return TypeUtils.GetPrimitiveType(this);
        }

        public bool IsVarArgs()
        {
            return (Flags & Reflection.ArgumentFlags.VarArgs) == Reflection.ArgumentFlags.VarArgs;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
