namespace FluidScript.Dynamic
{
    internal sealed class Function
    {
        public System.Delegate Reference { get; }
        public System.Type[] ParameterTypes { get; }

        public Function(System.Type[] parameterTypes, System.Delegate reference)
        {
            ParameterTypes = parameterTypes;
            Reference = reference;
        }

        public object Invoke(params object[] args)
        {
            //Todo type convert
            return Reference.DynamicInvoke(new object[] { args });
        }
    }
}
