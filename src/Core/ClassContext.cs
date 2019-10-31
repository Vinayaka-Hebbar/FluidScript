using FluidScript.Compiler;
using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Core
{
    public struct ClassContext : IInvocationContext
    {
        public readonly IOperationContext Context;
        public readonly ExpressionType ParentKind;

        public ClassContext(IOperationContext context, ExpressionType parentKind)
        {
            Context = context;
            ParentKind = parentKind;
        }

        public bool CanInvoke => true;

        public Object Invoke(Object target)
        {
            return Object.Void;
        }
    }
}
