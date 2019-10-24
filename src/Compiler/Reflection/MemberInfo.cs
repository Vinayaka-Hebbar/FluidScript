namespace FluidScript.Compiler.Reflection
{
    public abstract class MemberInfo
    {
        public TypeInfo DeclaredType { get; set; }

        protected MemberInfo(TypeInfo declaredType)
        {
            DeclaredType = declaredType;
        }

        protected MemberInfo()
        {
        }

        public abstract System.Reflection.MemberTypes Types { get; }
    }
}
