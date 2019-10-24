using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Core
{
    public interface IInvocationContext
    {
        bool CanInvoke { get; }
        Object Invoke(Object args);
    }

    public interface IMethodInvocation : IInvocationContext
    {
        Object Invoke(string name, NodeType type, object obj, object[] args);
    }

    public interface IPropertyInvocation : IInvocationContext
    {
        Object Invoke(string name, NodeType type, object obj);
    }
}
