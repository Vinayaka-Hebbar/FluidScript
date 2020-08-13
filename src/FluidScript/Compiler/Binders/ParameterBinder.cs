using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using System;

namespace FluidScript.Compiler.Binders
{
    #region Parameter Binder
    public
#if LATEST_VS
        readonly
#endif
        struct ParameterBinder : IBinder
    {
        readonly ParameterInfo parameter;

        public ParameterBinder(ParameterInfo parameter)
        {
            this.parameter = parameter;
        }

        public Type Type => parameter.Type;

        public BindingAttributes Attributes => BindingAttributes.None;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {
            if((option & MethodCompileOption.EmitStartAddress) == MethodCompileOption.EmitStartAddress && parameter.Type.IsValueType)
            {
                if (generator.Method.IsStatic)
                    generator.LoadAddressOfArgument(parameter.Index);
                else
                    generator.LoadAddressOfArgument(parameter.Index + 1);
            }
            else
            {
                if (generator.Method.IsStatic)
                    generator.LoadArgument(parameter.Index);
                else
                    generator.LoadArgument(parameter.Index + 1);
            }
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
        {
            if (generator.Method.IsStatic)
                generator.StoreArgument(parameter.Index);
            else
                generator.StoreArgument(parameter.Index + 1);
        }

        public object Get(object obj)
        {
            throw new NotSupportedException(nameof(Get));
        }

        public void Set(object obj, object value)
        {
            throw new NotSupportedException(nameof(Set));
        }
    }

    #endregion
}
