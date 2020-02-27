using System;

namespace FluidScript.Compiler.Emit
{
    public sealed class RuntimeParameterInfo : System.Reflection.ParameterInfo
    {
        private readonly ParameterInfo _parameter;
        public RuntimeParameterInfo(ParameterInfo parameter)
        {
            _parameter = parameter;
        }

        public override Type ParameterType => _parameter.Type;

        public override int Position => _parameter.Index;
    }
}
