using System;

namespace FluidScript.Compiler.Emit
{
    public struct Primitive
    {
        public static readonly Primitive Any = new Primitive(null, RuntimeType.Any);

        public readonly Type Type;
        public readonly RuntimeType Enum;

        public Primitive(Type type, RuntimeType @enum)
        {
            Type = type;
            Enum = @enum;
        }

        public override string ToString()
        {
            return Type.FullName;
        }
    }
}
