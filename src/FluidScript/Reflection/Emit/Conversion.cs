namespace FluidScript.Reflection.Emit
{
    /// <summary>
    /// Operator overload conversion
    /// </summary>
    public sealed class Conversion
    {
        internal const string ImplicitConversionName = "op_Implicit";

        internal const string ExplicitConviersionName = "op_Explicit";

        internal static readonly Conversion NoConversion = new Conversion(null);

        /// <summary>
        /// Conversion method
        /// </summary>
        public readonly System.Reflection.MethodInfo Method;

        /// <summary>
        /// Initializes new <see cref="Conversion"/>
        /// </summary>
        public Conversion(System.Reflection.MethodInfo method)
        {
            Method = method;
        }

        /// <summary>
        /// Indicates whether any conversion are there are not
        /// </summary>
        public bool HasConversion
        {
            get => Method != null;
        }

        internal object Convert(object value)
        {
            return Method.Invoke(null, new object[] { value });
        }

        internal void Generate(MethodBodyGenerator generator)
        {
            generator.CallStatic(Method);
        }
    }
}
