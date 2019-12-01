using System;

namespace FluidScript.Reflection
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class Callable : Accessable
    {
        public readonly ArgumentTypes[] Arguments;

        public RuntimeType ReturnType => Type;

        public Callable(string name, RuntimeType returnType, params ArgumentTypes[] args) : base(name, returnType)
        {
            Arguments = args;
        }
    }
}
