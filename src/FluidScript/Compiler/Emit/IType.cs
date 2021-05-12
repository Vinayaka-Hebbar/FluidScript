using System;

namespace FluidScript.Compiler.Emit
{
    public interface IType : IMember
    {
        Type UnderlyingSystemType { get; }

        Type BaseType { get; }

        Type CreateType();
    }

    public interface IRuntimeType
    {
        Type UnderlyingSystemType { get; }
    }
}
