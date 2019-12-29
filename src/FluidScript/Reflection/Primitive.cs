using System;

namespace FluidScript.Reflection
{
    public
#if LATEST_VS
        readonly
#endif
        struct Primitive
    {
        public static readonly Primitive Any = new Primitive("any", typeof(object));
        public readonly string Name;
        public readonly Type Type;

        public Primitive(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return Type.FullName;
        }
    }
}
