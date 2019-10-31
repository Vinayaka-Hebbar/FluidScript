using System;

namespace FluidScript.Compiler.Emit
{
    public struct Primitive
    {
        public static readonly Primitive Any = new Primitive(null, PrimitiveType.Any);

        public readonly Type Type;
        public readonly PrimitiveType Enum;

        public Primitive(Type type, PrimitiveType @enum)
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
