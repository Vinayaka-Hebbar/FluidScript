using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Reflection
{
    public class MethodInfo : MethodBase
    {
        public readonly string Name;
        public TypeInfo ReturnType { get; set; }

        internal MethodInfo(string name, TypeInfo returnType, MethodAttributes attributes) : base(attributes)
        {
            Name = name;
            ReturnType = returnType;
        }

        internal MethodInfo(string name, TypeInfo returnType, ParameterInfo[] parameters, TypeInfo declaredType, MethodAttributes attributes) : base(parameters, declaredType, attributes)
        {
            Name = name;
            ReturnType = returnType;
        }

        public MethodInfo(string name, MethodAttributes attributes):base(attributes)
        {
            Name = name;
        }

        public override MemberTypes Types => MemberTypes.Method;

        public override void Generate(TypeBuilder builder)
        {

        }
    }
}
