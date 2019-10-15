namespace FluidScript.Core
{
    public struct ObjectInvocation : IInvocationContext
    {
        public readonly IInvocationContext Context;

        public ObjectInvocation(IInvocationContext context)
        {
            Context = context;
        }

        public bool CanInvoke => true;

        public Object Invoke(Object target)
        {
            return target;
        }
    }
}
