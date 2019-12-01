namespace FluidScript.Reflection.Emit
{
    public class OptimizationInfo
    {
        private System.Type declaringType;

        public System.Reflection.Module Module { get; private set; }

        public OptimizationInfo(System.Type declaring)
        {
            DeclaringType = declaring;
        }

        protected OptimizationInfo(OptimizationInfo info)
        {
            DeclaringType = info.DeclaringType;
        }

        public System.Type DeclaringType
        {
            get => declaringType;
            private set
            {
                declaringType = value;
                Module = value.Module;
            }
        }

        public System.Type GetType(string typeName)
        {
            if (TypeUtils.IsInbuiltType(typeName))
                return TypeUtils.GetInbuiltType(typeName);
            return Module.GetType(typeName);
        }

        public System.Type GetType(ITypeInfo info, bool throwOnError = false)
        {
            return TypeUtils.GetType(info);
        }
    }
}
