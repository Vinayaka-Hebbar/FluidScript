namespace FluidScript.Runtime
{
    public struct TypedValue : System.Runtime.CompilerServices.IStrongBox
    {
        public object Value { get; set; }

        public readonly System.Type Type;

        public TypedValue(object value, System.Type type)
        {
            Value = value;
            Type = type;
        }
    }
}
