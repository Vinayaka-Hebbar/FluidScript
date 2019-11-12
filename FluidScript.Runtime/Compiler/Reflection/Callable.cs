using System;

namespace FluidScript.Compiler.Reflection
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class Callable : Attribute
    {
        public readonly string Name;

        public Callable(string name)
        {
            Name = name;
        }
    }
}
