using System;

namespace FluidScript.Compiler.Emit
{
    public interface IType : IMember
    {
        Type ReflectedType { get; }
        Type BaseType { get; }

        Type CreateType();
    }
}
