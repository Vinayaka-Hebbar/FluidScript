using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler
{
    public interface IProgramContext : ITypeProvider
    {
        void Register(string name, Type type);

        bool TryGetType(string name, out Type type);
    }
}
