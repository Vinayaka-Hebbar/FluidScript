namespace FluidScript.Core
{
    public struct MethodInvocation : IInvocationContext
    {
        public readonly IInvocationContext Context;
        public readonly Object[] Args;

        public MethodInvocation(IInvocationContext context, Object[] args)
        {
            Context = context;
            Args = args;
        }

        public bool CanInvoke => true;

        public Object Invoke(Object args)
        {
            if (Context.CanInvoke)
            {
                return Context.Invoke(args);
            }

            if (Context is TypeNameContext typeName)
            {
                //new
                var value = System.Activator.CreateInstance(typeName.Type());
                return new Object(value);
            }
            return Object.Null;
        }
    }
}
