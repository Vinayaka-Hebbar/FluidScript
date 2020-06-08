using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Runtime
{
    /// <summary>
    /// Compile provider for Expression
    /// </summary>
    public interface ICompileProvider
    {
        object Invoke(Expression node, object target = null);
    }
}
