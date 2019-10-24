using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Reflection
{
    public class ConstructorInfo : MethodBase
    {
        public static readonly ConstructorInfo[] Empty;

        static ConstructorInfo()
        {
            Empty = new ConstructorInfo[0];
        }

        public ConstructorInfo(ParameterInfo[] parameters, TypeInfo declaredType, MethodAttributes attributes) : base(parameters, declaredType, attributes)
        {
        }

        public override void Generate(TypeBuilder builder)
        {
            //todo
        }
    }
}