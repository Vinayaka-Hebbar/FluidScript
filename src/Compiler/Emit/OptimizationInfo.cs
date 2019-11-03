namespace FluidScript.Compiler.Emit
{
    public class OptimizationInfo
    {
        public readonly System.Func<string, bool, System.Type> ResolveType;
        public OptimizationInfo(System.Func<string, bool, System.Type> resolveType)
        {
            ResolveType = resolveType;
        }

        protected OptimizationInfo(Emit.OptimizationInfo info)
        {
            ResolveType = info.ResolveType;
        }

        private System.Type GetSystemType(TypeName name, bool throwOnError = false)
        {
            if (name.FullName == null)
                return null;
            if (TypeUtils.PrimitiveNames.ContainsKey(name.FullName))
            {
                if (name.IsArray())
                    TypeUtils.PrimitiveNames[name.FullName].Type.MakeArrayType();
                return TypeUtils.PrimitiveNames[name.FullName].Type;
            }
            System.Type type = System.Type.GetType(name.FullName, throwOnError);
            if (name.IsArray())
                return type.MakeArrayType();
            return type;
        }

        public System.Type GetType(TypeName name, bool throwOnError = false)
        {
            if (name.FullName == null)
                return null;
            if (TypeUtils.PrimitiveNames.ContainsKey(name.FullName))
            {
                if (name.IsArray())
                    return TypeUtils.PrimitiveNames[name.FullName].Type.MakeArrayType();
                return TypeUtils.PrimitiveNames[name.FullName].Type;
            }

            var type = ResolveType(name.FullName, throwOnError);
            if (type == null)
            {
                type = System.Type.GetType(name.FullName, throwOnError);
            }
            if (name.IsArray())
                return type.MakeArrayType();
            return type;
        }
    }
}
