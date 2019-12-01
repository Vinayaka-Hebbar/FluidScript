using System;

namespace FluidScript.Reflection.Emit
{
    public readonly struct InbuiltType
    {
        public static readonly InbuiltType Any = new InbuiltType("any", typeof(object), RuntimeType.Any);
        public readonly string Name;
        public readonly Type Type;
        public readonly RuntimeType Runtime;

        public InbuiltType(string name, Type type, RuntimeType @enum)
        {
            Name = name;
            Type = type;
            Runtime = @enum;
        }

        public override string ToString()
        {
            return Type.FullName;
        }
    }
}
