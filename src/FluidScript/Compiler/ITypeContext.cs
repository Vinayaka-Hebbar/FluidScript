using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler
{
    public interface ITypeContext
    {
        void Register(string name, Type type);

        bool TryGetType(string name, out Type type);

        /// <summary>
        /// Get resolved <see cref="Type"/>
        /// </summary>
        Type GetType(TypeName name);
    }
}
