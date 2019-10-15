namespace FluidScript.Core
{
    public struct ClassContext : IInvocationContext
    {
        public readonly IOperationContext Context;
        public readonly Expression.Operation ParentKind;

        public ClassContext(IOperationContext context, Expression.Operation parentKind)
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
