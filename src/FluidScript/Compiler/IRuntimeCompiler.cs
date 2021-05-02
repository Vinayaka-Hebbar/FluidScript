using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using System;

namespace FluidScript.Compiler
{
    public interface IRuntimeCompiler : ICompileProvider
    {
        ILocalVariables Locals { get; }
        IMemberResolver Resolver { get; set; }
        object Target { get; }

        IDisposable EnterScope();
        object Invoke(Statement statement, object target);
#if NETFRAMEWORK || NETCOREAPP2_0 || NETSTANDARD2_0
        object Invoke(Statement statement);
#else
        object Invoke(Statement statement) => Invoke(statement, CompilerBase.NoTarget);
#endif
    }
}