namespace FluidScript.Compiler.Emit
{
    public class TypeProvider
    {
        public readonly System.Func<string, bool, System.Type> OnProvide;

        public TypeProvider(System.Func<string, bool, System.Type> builder)
        {
            this.OnProvide = builder;
        }

        public System.Type GetType(string typeName)
        {
            return OnProvide(typeName, false);
        }

        public System.Type Provide(string typeName, bool throwOnError)
        {
            return OnProvide(typeName, throwOnError);
        }
    }
}
