using System;

namespace FluidScript.Compiler.Reflection
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Property : Accessable
    {
        public Property(string name, RuntimeType type) : base(name, type)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Field : Accessable
    {
        public Field(string name, RuntimeType type) : base(name, type)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class Accessable : Attribute
    {
        public readonly string Name;
        public readonly RuntimeType Type;

        protected Accessable(string name, RuntimeType type)
        {
            Name = name;
            Type = type;
        }
    }
}
