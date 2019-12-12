namespace FluidScript.Reflection.Emit
{
    internal class Conversion
    {
        internal const string ImplicitConversionName = "op_Implicit";

        internal const string ExplicitConviersionName = "op_Explicit";

        internal static Conversion NoConversion = new Conversion(null);

        internal readonly System.Reflection.MethodInfo Method;

        public Conversion(System.Reflection.MethodInfo method)
        {
            Method = method;
        }

        internal bool HasConversion
        {
            get => Method != null;
        }


    }
}
