using System;

namespace FluidScript.Compiler.Reflection
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class Callable : Accessable
    {
        public readonly Emit.ArgumentTypes[] Arguments;

        public RuntimeType ReturnType => Type;

        public Callable(string name, RuntimeType returnType, params Emit.ArgumentTypes[] args) : base(name, returnType)
        {
            Arguments = args;
        }
    }
}
