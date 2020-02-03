namespace FluidScript
{
    public sealed class Function : FSObject
    {
        public System.Delegate Reference { get; }
        public object[] ParameterTypes { get; }

        public Function(System.Type[] parameterTypes, System.Delegate reference)
        {
            ParameterTypes = parameterTypes;
            Reference = reference;
        }

        [Runtime.Register("invoke")]
        public object Invoke(params object[] args)
        {
            //Todo type convert
            return Reference.DynamicInvoke(Reference.Target, args);
        }
    }
}
