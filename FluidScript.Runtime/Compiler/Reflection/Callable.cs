using System;

namespace FluidScript.Compiler.Reflection
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class Callable : Attribute
    {
        public readonly string Name;

        public readonly Emit.ArgumentTypes[] Arguments;

        public RuntimeType ReturnType { get; }

        public Callable(string name)
        {
            Name = name;
        }

        public Callable(string name, Emit.ArgumentTypes[] args)
        {
            Name = name;
            Arguments = args;
        }

        public Callable(string name, RuntimeType returnType, params Emit.ArgumentTypes[] args)
        {
            Name = name;
            Arguments = args;
            ReturnType = returnType;
        }
    }
}
