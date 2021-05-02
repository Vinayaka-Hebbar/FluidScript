using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Runtime
{
    /// <summary>
    /// Compile provider for Expression and Statement
    /// </summary>
    public interface ICompileProvider
    {
        object Invoke(Expression node, object target);
#if NETFRAMEWORK || NETCOREAPP2_0 || NETSTANDARD2_0
        object Invoke(Expression node);
#else
        object Invoke(Expression node) => Invoke(node, Compiler.CompilerBase.NoTarget);
#endif
    }
}
