using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using System;

namespace FluidScript.Compiler.Binders
{
    #region Variable Binder
    public
#if LATEST_VS
        readonly
#endif
        struct VariableBinder : IBinder
    {
        readonly ILLocalVariable variable;

        public VariableBinder(ILLocalVariable variable)
        {
            this.variable = variable;
        }

        public Type Type => variable.Type;

        public BindingAttributes Attributes => BindingAttributes.None;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {
            if ((option & MethodCompileOption.EmitStartAddress) == 0)
                generator.LoadVariable(variable);
            else
                generator.LoadAddressOfVariable(variable);
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
        {
            generator.StoreVariable(variable);
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
