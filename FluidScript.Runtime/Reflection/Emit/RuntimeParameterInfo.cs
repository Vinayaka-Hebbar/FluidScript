using System;

namespace FluidScript.Reflection.Emit
{
    public class RuntimeParameterInfo : System.Reflection.ParameterInfo
    {
        public RuntimeParameterInfo(Type parameterType)
        {
            ParameterType = parameterType;
        }

        public override Type ParameterType { get; }
    }
}
