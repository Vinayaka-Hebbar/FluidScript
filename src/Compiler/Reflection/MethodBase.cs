using System;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Reflection
{
    public abstract class MethodBase : MemberInfo, IReflection
    {
        public ParameterInfo[] Parameters { get; set; }
        public System.Reflection.MethodAttributes Attributes { get; set; }

        protected MethodBase(System.Reflection.MethodAttributes attributes) : base()
        {
            Attributes = attributes;
        }

        protected MethodBase(ParameterInfo[] parameters, TypeInfo declaredType, System.Reflection.MethodAttributes attributes) : base(declaredType)
        {
            Parameters = parameters;
            Attributes = attributes;
        }

        private MethodBody Body;

        public virtual MethodBody GetMethodBody()
        {
            if (Body == null)
                Body = new MethodBody(this);
            return Body;
        }

        public abstract void Generate(TypeBuilder builder);
    }
}
